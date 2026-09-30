using Google;
using Integrations.Google.API.Drive.Mapping;
using Integrations.Google.API.Drive.Models;

namespace Integrations.Google.API.Drive.Services;

internal partial class GoogleDriveService
{
    public async Task<DriveFile?> GetFileAsync(
        int userId,
        string fileId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            throw new ArgumentException(
                "Identyfikator pliku nie może być pusty.",
                nameof(fileId));
        }

        try
        {
            var service = await _googleServiceFactory.CreateDriveAsync(
                userId,
                cancellationToken);

            var request = service.Files.Get(fileId);

            request.Fields =
                "id, name, description, mimeType, size, createdTime, " +
                "modifiedTime, webViewLink, webContentLink, parents, " +
                "trashed, iconLink, thumbnailLink";

            var file = await request.ExecuteAsync(cancellationToken);

            return file is null
                ? null
                : GoogleDriveMapper.Map(file);
        }
        catch (GoogleApiException ex) when (
            ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}