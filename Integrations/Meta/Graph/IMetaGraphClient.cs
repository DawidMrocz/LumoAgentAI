namespace Integrations.Meta.Graph
{
    public interface IMetaGraphClient
    {
        Task<T?> GetAsync<T>(
            string path,
            int userId,
            Dictionary<string, string>? query = null,
            CancellationToken cancellationToken = default);

        Task<T?> PostAsync<T>(
            string path,
            int userId,
            Dictionary<string, string> formValues,
            CancellationToken cancellationToken = default);
    }
}
