namespace Lumo.Api.Services.Agent
{
    /// <summary>
    /// Cennik modeli (USD za 1 mln tokenów) do szacowania kosztu wywołań.
    /// Konfiguracja: AI:Pricing:&lt;model&gt;:Input / :Output / :CachedInput
    /// Model w kluczu to sama nazwa modelu, np. "gpt-4o-mini".
    /// </summary>
    public sealed class AiPricing
    {
        private readonly IConfiguration _configuration;

        public AiPricing(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Wbudowane stawki (USD / 1 mln tokenów) dla popularnych modeli, żeby szacowanie
        /// kosztu działało bez dodatkowej konfiguracji. Konfiguracja AI:Pricing ma pierwszeństwo.
        /// Cenniki dostawców się zmieniają - traktuj to jako oszacowanie, nie fakturę.
        /// </summary>
        private static readonly Dictionary<string, (decimal Input, decimal CachedInput, decimal Output)> DefaultPricing =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["gpt-4o-mini"] = (0.15m, 0.075m, 0.60m),
                ["gpt-4o"] = (2.50m, 1.25m, 10.00m),
                ["gpt-4.1-mini"] = (0.40m, 0.10m, 1.60m),
                ["gpt-4.1"] = (2.00m, 0.50m, 8.00m)
            };

        public decimal? EstimateUsd(string model, long inputTokens, long cachedInputTokens, long outputTokens)
        {
            var section = _configuration.GetSection($"AI:Pricing:{model}");

            var inputPer1M = ReadDecimal(section, "Input");
            var outputPer1M = ReadDecimal(section, "Output");

            if (inputPer1M is null && outputPer1M is null)
            {
                if (!DefaultPricing.TryGetValue(model, out var fallback))
                    return null;

                inputPer1M = fallback.Input;
                outputPer1M = fallback.Output;

                var uncached = Math.Max(0, inputTokens - cachedInputTokens);

                return (uncached / 1_000_000m) * fallback.Input
                     + (cachedInputTokens / 1_000_000m) * fallback.CachedInput
                     + (outputTokens / 1_000_000m) * fallback.Output;
            }

            // Tokeny z cache są zwykle tańsze; gdy brak osobnej stawki, liczymy jak zwykłe wejście.
            var cachedPer1M = ReadDecimal(section, "CachedInput") ?? inputPer1M;

            // CachedInputTokenCount jest PODZBIOREM InputTokenCount - nie liczymy go dwa razy.
            var uncachedInput = Math.Max(0, inputTokens - cachedInputTokens);

            var cost =
                (uncachedInput / 1_000_000m) * (inputPer1M ?? 0m) +
                (cachedInputTokens / 1_000_000m) * (cachedPer1M ?? 0m) +
                (outputTokens / 1_000_000m) * (outputPer1M ?? 0m);

            return cost;
        }

        private static decimal? ReadDecimal(IConfigurationSection section, string key)
        {
            var raw = section[key];

            return decimal.TryParse(
                raw,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var value)
                ? value
                : null;
        }
    }
}
