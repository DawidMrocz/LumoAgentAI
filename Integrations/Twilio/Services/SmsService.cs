using Integrations.Twilio.Client;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace Integrations.Twilio.Services
{
    internal sealed class SmsService : ISmsService
    {
        private static readonly Dictionary<string, string> Templates = new()
        {
            ["welcome"] = "Witaj {name}! Dziękujemy za rejestrację w Lumo.",
            ["verification"] = "Twój kod weryfikacyjny: {code}. Ważny przez {minutes} minut.",
            ["password_reset"] = "Kod resetowania hasła: {code}. Jeśli to nie Ty, zignoruj tę wiadomość.",
            ["notification"] = "{message}"
        };

        private readonly ITwilioClient _twilioClient;
        private readonly ISmsConfigRepository _configRepository;
        private readonly ISmsMessageRepository _messageRepository;
        private readonly ILogger<SmsService> _logger;

        public SmsService(
            ITwilioClient twilioClient,
            ISmsConfigRepository configRepository,
            ISmsMessageRepository messageRepository,
            ILogger<SmsService> logger)
        {
            _twilioClient = twilioClient;
            _configRepository = configRepository;
            _messageRepository = messageRepository;
            _logger = logger;
        }

        public async Task<SendSmsResponse> SendSmsAsync(
            SendSmsRequest request,
            int? userId = null,
            CancellationToken cancellationToken = default)
        {
            var config = await _configRepository.GetAsync(cancellationToken)
                ?? throw new InvalidOperationException("Twilio: Brak konfiguracji SMS.");

            var parts = CalculateSmsParts(request.Message);

            var entity = new SmsMessage
            {
                UserId = userId,
                To = request.To,
                Sender = request.Sender ?? config.PhoneNumber,
                Message = request.Message,
                Parts = parts,
                IsFlash = request.IsFlash,
                IsTest = config.TestMode,
                ScheduledAt = request.ScheduledAt,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = SmsStatus.Queued
            };

            try
            {
                if (config.TestMode)
                {
                    entity.Status = SmsStatus.Sent;
                    entity.SentAt = DateTimeOffset.UtcNow;
                    entity.ExternalId = $"TEST-{Guid.NewGuid():N}";
                }
                else
                {
                    var message = await _twilioClient.SendAsync(
                        request.To,
                        config.PhoneNumber,
                        request.Message,
                        request.ScheduledAt,
                        cancellationToken);

                    entity.ExternalId = message.Sid;
                    entity.Status = SmsStatus.Sent;
                    entity.SentAt = DateTimeOffset.UtcNow;
                }

                await _messageRepository.AddAsync(entity, cancellationToken);

                return new SendSmsResponse
                {
                    MessageId = entity.ExternalId ?? string.Empty,
                    To = request.To,
                    Parts = parts,
                    Status = entity.Status.ToString(),
                    SentAt = entity.SentAt ?? DateTimeOffset.UtcNow,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas wysyłania SMS do {To}", request.To);

                entity.Status = SmsStatus.Failed;
                entity.ErrorMessage = ex.Message;

                await _messageRepository.AddAsync(entity, cancellationToken);

                return new SendSmsResponse
                {
                    To = request.To,
                    Parts = parts,
                    Status = entity.Status.ToString(),
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<SendBulkSmsResponse> SendBulkSmsAsync(
            SendBulkSmsRequest request,
            int? userId = null,
            CancellationToken cancellationToken = default)
        {
            var response = new SendBulkSmsResponse();

            foreach (var to in request.To)
            {
                var result = await SendSmsAsync(
                    new SendSmsRequest
                    {
                        To = to,
                        Message = request.Message,
                        Sender = request.Sender,
                        ScheduledAt = request.ScheduledAt,
                        Variables = request.Variables
                    },
                    userId,
                    cancellationToken);

                response.Results.Add(result);

                if (result.Success)
                    response.SuccessCount++;
                else
                    response.FailureCount++;

                response.TotalPoints += result.Points;
            }

            return response;
        }

        public Task<SendSmsResponse> SendFromTemplateAsync(
            string templateName,
            string to,
            Dictionary<string, string> variables,
            int? userId = null,
            CancellationToken cancellationToken = default)
        {
            if (!Templates.TryGetValue(templateName, out var content))
            {
                throw new ArgumentException($"Nieznany szablon SMS: {templateName}", nameof(templateName));
            }

            var message = variables.Aggregate(
                content,
                (current, variable) => current.Replace($"{{{variable.Key}}}", variable.Value));

            return SendSmsAsync(
                new SendSmsRequest { To = to, Message = message },
                userId,
                cancellationToken);
        }

        public async Task<SmsStatus> GetMessageStatusAsync(
            string messageId,
            CancellationToken cancellationToken = default)
        {
            var message = await _messageRepository.GetByExternalIdAsync(messageId, cancellationToken)
                ?? throw new ArgumentException($"Nie znaleziono wiadomości: {messageId}", nameof(messageId));

            return message.Status;
        }

        public async Task<List<SmsMessageInfo>> GetUserMessagesAsync(
            int userId,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
        {
            var messages = await _messageRepository.GetByUserIdAsync(userId, skip, take, cancellationToken);

            return messages.Select(MapToInfo).ToList();
        }

        public async Task<SmsStatistics> GetStatisticsAsync(
            int? userId = null,
            DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            CancellationToken cancellationToken = default)
        {
            var messages = await _messageRepository.GetInRangeAsync(userId, from, to, cancellationToken);

            return new SmsStatistics
            {
                TotalMessages = messages.Count,
                DeliveredCount = messages.Count(m => m.Status is SmsStatus.Delivered or SmsStatus.Sent),
                FailedCount = messages.Count(m => m.Status == SmsStatus.Failed),
                TotalPoints = messages.Sum(m => m.Points)
            };
        }

        private static SmsMessageInfo MapToInfo(SmsMessage message) => new()
        {
            Id = message.Id,
            ExternalId = message.ExternalId,
            To = message.To,
            Message = message.Message,
            Status = message.Status,
            Parts = message.Parts,
            Points = message.Points,
            CreatedAt = message.CreatedAt,
            SentAt = message.SentAt,
            DeliveredAt = message.DeliveredAt
        };

        private static int CalculateSmsParts(string message)
        {
            if (message.Length == 0)
                return 0;

            var hasUnicode = message.Any(c => c > 127);

            if (hasUnicode)
            {
                return message.Length <= 70
                    ? 1
                    : (int)Math.Ceiling((double)message.Length / 67);
            }

            return message.Length <= 160
                ? 1
                : (int)Math.Ceiling((double)message.Length / 153);
        }
    }
}
