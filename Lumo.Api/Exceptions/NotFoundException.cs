namespace Lumo.Api.Exceptions
{
    // ======================
    // 404 - Not Found
    // ======================
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
