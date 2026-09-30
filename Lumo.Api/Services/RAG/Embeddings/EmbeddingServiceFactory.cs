using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lumo.Api.Services.RAG.Embeddings
{
    public sealed class EmbeddingServiceFactory : IEmbeddingServiceFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public EmbeddingServiceFactory(
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public IEmbeddingService Create()
        {
            var provider = _configuration["AI:EmbeddingProvider"]
                ?? throw new InvalidOperationException(
                    "AI:EmbeddingProvider is not configured.");

            return provider.ToLowerInvariant() switch
            {
                "openai" =>
                    _serviceProvider.GetRequiredService<OpenAIEmbeddingService>(),

                "azureopenai" =>
                    _serviceProvider.GetRequiredService<AzureOpenAIEmbeddingService>(),

                "ollama" =>
                    _serviceProvider.GetRequiredService<OllamaEmbeddingService>(),

                _ => throw new InvalidOperationException(
                    $"Unsupported embedding provider: '{provider}'.")
            };
        }
    }
}
