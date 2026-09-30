using Lumo.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.Twilio.Services
{
    public interface ISmsService
    {
        Task<SendSmsResponse> SendSmsAsync(
            SendSmsRequest request,
            int? userId = null,
            CancellationToken cancellationToken = default);

        Task<SendBulkSmsResponse> SendBulkSmsAsync(
            SendBulkSmsRequest request,
            int? userId = null,
            CancellationToken cancellationToken = default);

        Task<SendSmsResponse> SendFromTemplateAsync(
            string templateName,
            string to,
            Dictionary<string, string> variables,
            int? userId = null,
            CancellationToken cancellationToken = default);

        Task<SmsStatus> GetMessageStatusAsync(
            string messageId,
            CancellationToken cancellationToken = default);

        Task<List<SmsMessageInfo>> GetUserMessagesAsync(
            int userId,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default);

        Task<SmsStatistics> GetStatisticsAsync(
            int? userId = null,
            DateTimeOffset? from = null,
            DateTimeOffset? to = null,
            CancellationToken cancellationToken = default);
    }
}
