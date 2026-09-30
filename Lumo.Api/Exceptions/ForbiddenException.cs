namespace Lumo.Api.Exceptions
{
    // ======================
    // 403 - Forbidden
    // ======================
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
