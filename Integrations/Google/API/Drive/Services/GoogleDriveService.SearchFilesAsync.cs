using Integrations.Google.API.Drive.Mapping;
using Integrations.Google.API.Drive.Models;

namespace Integrations.Google.API.Drive.Services;

internal partial class GoogleDriveService
{
    public async Task<IReadOnlyList<DriveFile>> SearchFilesAsync(
        int userId,
        string query,
        int maxResults = 50,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Zapytanie wyszukiwania nie może być puste.", nameof(query));
        }

        var service = await _googleServiceFactory.CreateDriveAsync(userId, cancellationToken);
        var request = service.Files.List();

        request.Fields = "nextPageToken, files(id, name, description, mimeType, size, createdTime, modifiedTime, webViewLink, webContentLink, parents, trashed, iconLink, thumbnailLink)";

        // Wyszukiwanie w nazwie i pełnej zawartości tekstu, z wykluczeniem plików w koszu
        var escapedQuery = query.Replace("'", "\\'");
        request.Q = $"(name contains '{escapedQuery}' or fullText contains '{escapedQuery}') and trashed = false";

        request.PageSize = Math.Min(maxResults, 1000);
        request.OrderBy = "modifiedTime desc";

        var result = new List<DriveFile>();
        string? pageToken = null;

        do
        {
            request.PageToken = pageToken;
            var response = await request.ExecuteAsync(cancellationToken);

            if (response.Files is not null)
            {
                result.AddRange(response.Files.Select(GoogleDriveMapper.Map));
            }

            pageToken = response.NextPageToken;

            if (result.Count >= maxResults)
            {
                break;
            }

        } while (!string.IsNullOrWhiteSpace(pageToken));

        return result.Take(maxResults).ToList();
    }
}
