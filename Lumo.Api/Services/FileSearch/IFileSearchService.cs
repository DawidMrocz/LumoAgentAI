using Microsoft.Agents.AI;

namespace Lumo.Api.Services.FileSearch
{
    public interface IFileSearchService
    {
        Task<string> AddFileAsync(string filePath, string fileName);
        Task<string> SearchAsync(string vectorStoreId, string query);
    }

    public class FileSearchResult
    {
        public string FileId { get; init; } = string.Empty;

        public string FileName { get; init; } = string.Empty;

        public string Content { get; init; } = string.Empty;
    }
}
