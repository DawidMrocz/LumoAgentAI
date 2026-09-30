using Integrations.Google.API.Drive.Mapping;
using Integrations.Google.API.Drive.Models;

namespace Integrations.Google.API.Drive.Services;

internal partial class GoogleDriveService
{
    public async Task<IReadOnlyList<DriveFile>> ListFilesAsync(
        int userId,
        string? parentFolderId = null,
        int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        var service = await _googleServiceFactory.CreateDriveAsync(userId, cancellationToken);
        var request = service.Files.List();

        // Podstawowe pola do pobrania
        request.Fields = "nextPageToken, files(id, name, description, mimeType, size, createdTime, modifiedTime, webViewLink, webContentLink, parents, trashed, iconLink, thumbnailLink)";

        // Filtr: tylko pliki nie w koszu
        var query = "trashed = false";

        // Jeśli podano folder nadrzędny, filtruj po nim
        if (!string.IsNullOrWhiteSpace(parentFolderId))
        {
            query += $" and '{parentFolderId}' in parents";
        }

        request.Q = query;
        request.PageSize = Math.Min(maxResults, 1000); // Google Drive API max to 1000
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

            // Przerwij, jeśli osiągnięto maxResults
            if (result.Count >= maxResults)
            {
                break;
            }

        } while (!string.IsNullOrWhiteSpace(pageToken));

        return result.Take(maxResults).ToList();
    }
}
