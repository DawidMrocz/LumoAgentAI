using Lumo.Api.Services.AgentUserContext;
using System.Security.Claims;

namespace Lumo.Api.Middlewares
{
    /// <summary>
    /// Ustawia kontekst użytkownika (AsyncLocal) na czas obsługi żądania.
    /// Bez tego IAgentUserContextService działał wyłącznie wewnątrz rozmowy z agentem,
    /// a każdy inny serwis z niego korzystający (np. RAG w DocumentsController)
    /// wywracał się na "Agent user context is not initialized".
    /// </summary>
    public sealed class AgentUserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public AgentUserContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IAgentUserContextService userContext)
        {
            var userId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                await _next(httpContext);
                return;
            }

            using var scope = userContext.Push(userId);

            await _next(httpContext);
        }
    }
}
