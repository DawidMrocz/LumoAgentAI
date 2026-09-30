using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace Lumo.Api.Services.Agent
{
    /// <summary>
    /// Dekorator IChatClient zbierający telemetrię wywołań modelu:
    /// liczba żądań/błędów, czas trwania, czas do pierwszego tokena,
    /// zużycie tokenów (wejście / cache / wyjście / reasoning), szacowany koszt
    /// oraz liczba wywołań narzędzi.
    ///
    /// Obsługuje ZARÓWNO tryb zwykły, JAK I streaming - wcześniej ścieżka
    /// streamingowa (główna dla czatu) przechodziła bez żadnej telemetrii.
    /// </summary>
    public sealed class ObservabilityChatClient : IChatClient
    {
        private readonly IChatClient _inner;
        private readonly ILogger<ObservabilityChatClient> _logger;
        private readonly AiPricing _pricing;
        private readonly string _model;
        private readonly string _modelName;

        private readonly Counter<long> _requests;
        private readonly Counter<long> _errors;
        private readonly Histogram<double> _duration;
        private readonly Histogram<double> _timeToFirstToken;
        private readonly Counter<long> _tokens;
        private readonly Counter<long> _toolCalls;
        private readonly Counter<double> _cost;

        public ObservabilityChatClient(
            IChatClient inner,
            ILoggerFactory loggerFactory,
            Meter meter,
            string model,
            AiPricing pricing)
        {
            _inner = inner;
            _logger = loggerFactory.CreateLogger<ObservabilityChatClient>();
            _pricing = pricing;
            _model = model;

            // "provider:model" -> sama nazwa modelu, potrzebna do cennika.
            var separator = model.LastIndexOf(':');
            _modelName = separator >= 0 && separator < model.Length - 1
                ? model[(separator + 1)..]
                : model;

            _requests = meter.CreateCounter<long>("lumo.ai.chat.requests", description: "Number of AI chat requests.");
            _errors = meter.CreateCounter<long>("lumo.ai.chat.errors", description: "Number of failed AI chat requests.");
            _duration = meter.CreateHistogram<double>("lumo.ai.chat.duration", unit: "s", description: "AI chat request duration.");
            _timeToFirstToken = meter.CreateHistogram<double>("lumo.ai.chat.ttft", unit: "s", description: "Time to first streamed token.");
            _tokens = meter.CreateCounter<long>("lumo.ai.tokens", unit: "{token}", description: "Tokens consumed, tagged by type.");
            _toolCalls = meter.CreateCounter<long>("lumo.ai.tool_calls", description: "Number of tool calls requested by the model.");
            _cost = meter.CreateCounter<double>("lumo.ai.cost.usd", unit: "USD", description: "Estimated spend.");
        }

        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var messageList = messages as IReadOnlyCollection<ChatMessage> ?? messages.ToArray();

            var stopwatch = Stopwatch.StartNew();
            _requests.Add(1, ModelTag, StreamingTag(false));

            _logger.LogInformation(
                "AI request started. Model={Model}, Messages={MessageCount}, Streaming={Streaming}",
                _model, messageList.Count, false);

            try
            {
                var response = await _inner.GetResponseAsync(messageList, options, cancellationToken);

                stopwatch.Stop();
                RecordDuration(stopwatch, streaming: false);

                var toolCalls = CountToolCalls(response.Messages.SelectMany(m => m.Contents));

                RecordUsage(response.Usage, toolCalls, stopwatch, streaming: false, finishReason: response.FinishReason?.Value);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                RecordError(ex, stopwatch, streaming: false);
                throw;
            }
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var messageList = messages as IReadOnlyCollection<ChatMessage> ?? messages.ToArray();

            var stopwatch = Stopwatch.StartNew();
            _requests.Add(1, ModelTag, StreamingTag(true));

            _logger.LogInformation(
                "AI request started. Model={Model}, Messages={MessageCount}, Streaming={Streaming}",
                _model, messageList.Count, true);

            UsageDetails? usage = null;
            var toolCalls = 0;
            var firstTokenRecorded = false;
            string? finishReason = null;

            IAsyncEnumerator<ChatResponseUpdate> enumerator;

            try
            {
                enumerator = _inner
                    .GetStreamingResponseAsync(messageList, options, cancellationToken)
                    .GetAsyncEnumerator(cancellationToken);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                RecordError(ex, stopwatch, streaming: true);
                throw;
            }

            try
            {
                while (true)
                {
                    ChatResponseUpdate update;

                    try
                    {
                        if (!await enumerator.MoveNextAsync())
                            break;

                        update = enumerator.Current;
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        RecordError(ex, stopwatch, streaming: true);
                        throw;
                    }

                    if (!firstTokenRecorded && !string.IsNullOrEmpty(update.Text))
                    {
                        firstTokenRecorded = true;

                        _timeToFirstToken.Record(
                            stopwatch.Elapsed.TotalSeconds,
                            ModelTag);
                    }

                    // Zużycie tokenów przychodzi w streamie jako UsageContent,
                    // zwykle w ostatnich porcjach odpowiedzi.
                    foreach (var content in update.Contents)
                    {
                        if (content is UsageContent usageContent)
                        {
                            usage = Merge(usage, usageContent.Details);
                        }
                        else if (content is FunctionCallContent)
                        {
                            toolCalls++;
                        }
                    }

                    finishReason ??= update.FinishReason?.Value;

                    yield return update;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            stopwatch.Stop();
            RecordDuration(stopwatch, streaming: true);
            RecordUsage(usage, toolCalls, stopwatch, streaming: true, finishReason: finishReason);
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
            => _inner.GetService(serviceType, serviceKey);

        public void Dispose() => _inner.Dispose();

        // ------------------------------------------------------------
        // Telemetria
        // ------------------------------------------------------------

        private KeyValuePair<string, object?> ModelTag => new("model", _model);

        private static KeyValuePair<string, object?> StreamingTag(bool streaming)
            => new("streaming", streaming);

        private void RecordDuration(Stopwatch stopwatch, bool streaming)
        {
            _duration.Record(stopwatch.Elapsed.TotalSeconds, ModelTag, StreamingTag(streaming));
        }

        private void RecordError(Exception ex, Stopwatch stopwatch, bool streaming)
        {
            // Anulowanie przez klienta to nie awaria modelu - nie zawyżamy licznika błędów.
            var canceled = ex is OperationCanceledException;

            if (!canceled)
                _errors.Add(1, ModelTag, StreamingTag(streaming), new KeyValuePair<string, object?>("exception", ex.GetType().Name));

            RecordDuration(stopwatch, streaming);

            if (canceled)
            {
                _logger.LogInformation(
                    "AI request canceled. Model={Model}, Streaming={Streaming}, DurationMs={DurationMs:F0}",
                    _model, streaming, stopwatch.Elapsed.TotalMilliseconds);
            }
            else
            {
                _logger.LogError(ex,
                    "AI request failed. Model={Model}, Streaming={Streaming}, DurationMs={DurationMs:F0}",
                    _model, streaming, stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        private void RecordUsage(
            UsageDetails? usage,
            int toolCalls,
            Stopwatch stopwatch,
            bool streaming,
            string? finishReason)
        {
            if (toolCalls > 0)
                _toolCalls.Add(toolCalls, ModelTag);

            if (usage is null)
            {
                _logger.LogInformation(
                    "AI request completed (brak danych o zużyciu tokenów). Model={Model}, Streaming={Streaming}, " +
                    "DurationMs={DurationMs:F0}, ToolCalls={ToolCalls}, FinishReason={FinishReason}",
                    _model, streaming, stopwatch.Elapsed.TotalMilliseconds, toolCalls, finishReason);

                return;
            }

            var input = usage.InputTokenCount ?? 0;
            var output = usage.OutputTokenCount ?? 0;
            var cached = usage.CachedInputTokenCount ?? 0;
            var reasoning = usage.ReasoningTokenCount ?? 0;
            var total = usage.TotalTokenCount ?? (input + output);

            if (input > 0) _tokens.Add(input, ModelTag, new KeyValuePair<string, object?>("type", "input"));
            if (output > 0) _tokens.Add(output, ModelTag, new KeyValuePair<string, object?>("type", "output"));
            if (cached > 0) _tokens.Add(cached, ModelTag, new KeyValuePair<string, object?>("type", "cached_input"));
            if (reasoning > 0) _tokens.Add(reasoning, ModelTag, new KeyValuePair<string, object?>("type", "reasoning"));

            var costUsd = _pricing.EstimateUsd(_modelName, input, cached, output);

            if (costUsd is { } cost)
                _cost.Add((double)cost, ModelTag);

            // Odsetek promptu obsłużony z cache - bezpośrednia miara skuteczności
            // cache'owania promptu (im wyżej, tym taniej).
            var cacheHitRate = input > 0 ? (double)cached / input : 0d;

            _logger.LogInformation(
                "AI request completed. Model={Model}, Streaming={Streaming}, DurationMs={DurationMs:F0}, " +
                "InputTokens={InputTokens}, CachedInputTokens={CachedInputTokens}, CacheHitRate={CacheHitRate:P1}, " +
                "OutputTokens={OutputTokens}, ReasoningTokens={ReasoningTokens}, TotalTokens={TotalTokens}, " +
                "ToolCalls={ToolCalls}, FinishReason={FinishReason}, EstimatedCostUsd={EstimatedCostUsd}",
                _model, streaming, stopwatch.Elapsed.TotalMilliseconds,
                input, cached, cacheHitRate, output, reasoning, total,
                toolCalls, finishReason,
                costUsd is null ? "n/d" : costUsd.Value.ToString("F6"));

            // Ucięta odpowiedź to realny problem jakościowy - MaxOutputTokens za niskie.
            if (string.Equals(finishReason, "length", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Odpowiedź modelu została ucięta limitem MaxOutputTokens. Model={Model}, OutputTokens={OutputTokens}",
                    _model, output);
            }
        }

        private static int CountToolCalls(IEnumerable<AIContent> contents)
            => contents.Count(c => c is FunctionCallContent);

        /// <summary>
        /// W streamingu porcje z UsageContent mogą przyjść wielokrotnie (np. po każdej
        /// turze narzędzi) - sumujemy je, zamiast nadpisywać.
        /// </summary>
        private static UsageDetails Merge(UsageDetails? accumulated, UsageDetails incoming)
        {
            if (accumulated is null)
                return incoming;

            return new UsageDetails
            {
                InputTokenCount = Add(accumulated.InputTokenCount, incoming.InputTokenCount),
                OutputTokenCount = Add(accumulated.OutputTokenCount, incoming.OutputTokenCount),
                TotalTokenCount = Add(accumulated.TotalTokenCount, incoming.TotalTokenCount),
                CachedInputTokenCount = Add(accumulated.CachedInputTokenCount, incoming.CachedInputTokenCount),
                ReasoningTokenCount = Add(accumulated.ReasoningTokenCount, incoming.ReasoningTokenCount)
            };

            static long? Add(long? a, long? b)
                => a is null && b is null ? null : (a ?? 0) + (b ?? 0);
        }
    }
}
