namespace Lumo.Api.Exceptions
{
    // ======================
    // 401 - Unauthorized (opcjonalnie)
    // ======================
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}
