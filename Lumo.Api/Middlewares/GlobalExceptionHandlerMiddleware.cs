using Lumo.Api.Exceptions;

namespace Lumo.Api.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                httpContext.Request.EnableBuffering();
                await _next(httpContext);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(httpContext, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.ContentType = "application/json";

            switch (exception)
            {
                // ======================
                // 400 - VALIDATION
                // ======================
                case BusinessException ex:
                    await WriteResponse(httpContext, 400, ex.Message);
                    return;

                // ======================
                // 404 - NOT FOUND
                // ======================
                case NotFoundException:
                    await WriteResponse(httpContext, 404,
                        "Nie znaleziono rekordu o podanym parametrze");
                    return;

                // ======================
                // 401 - UNAUTHORIZED
                // ======================
                case UnauthorizedException ex:
                    await WriteResponse(httpContext, 401,
                        string.IsNullOrWhiteSpace(ex.Message)
                            ? "Użytkownik nie jest zalogowany"
                            : ex.Message);
                    return;

                // ======================
                // 403 - FORBIDDEN
                // ======================
                case ForbiddenException ex:
                    await WriteResponse(httpContext, 403,
                        string.IsNullOrWhiteSpace(ex.Message)
                            ? "Brak dostępu"
                            : ex.Message);
                    return;
            }

            _logger.LogError(exception, "Nieobsłużony wyjątek podczas przetwarzania {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            await WriteResponse(httpContext, 500,
                "Przepraszamy wystąpił błąd w naszym serwisie");
        }

        private async Task WriteResponse(HttpContext httpContext, int status, string message)
        {
            httpContext.Response.StatusCode = status;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                status,
                message
            });
        }
    }
}