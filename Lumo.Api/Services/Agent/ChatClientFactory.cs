using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace Lumo.Api.Services.Agent
{
    public sealed class ChatClientFactory : IChatClientFactory
    {
        private readonly IConfiguration _configuration;

        public ChatClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IChatClient Create()
        {
            var provider = _configuration["AI:ChatProvider"]
                ?? throw new InvalidOperationException(
                    "AI:ChatProvider is not configured.");

            return provider.ToLowerInvariant() switch
            {
                "openai" => CreateOpenAI(),
                "azureopenai" => CreateAzureOpenAI(),
                "ollama" => CreateOllama(),

                _ => throw new InvalidOperationException(
                    $"Unsupported chat provider: '{provider}'.")
            };
        }

        private IChatClient CreateOpenAI()
        {
            var apiKey = _configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException(
                    "OpenAI:ApiKey is not configured.");

            var model = _configuration["OpenAI:Model"]
                ?? throw new InvalidOperationException(
                    "OpenAI:Model is not configured.");

            return new OpenAIClient(apiKey).GetChatClient(model).AsIChatClient();
        }

        private IChatClient CreateAzureOpenAI()
        {
            var endpoint = _configuration["AzureOpenAI:Endpoint"]
                ?? throw new InvalidOperationException(
                    "AzureOpenAI:Endpoint is not configured.");

            var apiKey = _configuration["AzureOpenAI:ApiKey"]
                ?? throw new InvalidOperationException(
                    "AzureOpenAI:ApiKey is not configured.");

            var deployment = _configuration["AzureOpenAI:ChatDeployment"]
                ?? throw new InvalidOperationException(
                    "AzureOpenAI:ChatDeployment is not configured.");

            var client = new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));

            return client.GetChatClient(deployment).AsIChatClient();
        }

        private IChatClient CreateOllama()
        {
            var baseUrl = _configuration["Ollama:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "Ollama:BaseUrl is not configured.");

            var model = _configuration["Ollama:GenerateModel"]
                ?? throw new InvalidOperationException(
                    "Ollama:GenerateModel is not configured.");

            var client = new OpenAIClient(
                new ApiKeyCredential("local"),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri($"{baseUrl.TrimEnd('/')}")
                });

            return client
                .GetChatClient(model)
                .AsIChatClient();
        }
    }

    public interface IChatClientFactory
    {
        IChatClient Create();
    }
}
