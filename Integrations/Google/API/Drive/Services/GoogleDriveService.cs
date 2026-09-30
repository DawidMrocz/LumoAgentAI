using Google.Apis.Drive.v3.Data;
using Integrations.Google.API.Drive.Mapping;
using Integrations.Google.API.Drive.Models;
using Integrations.Google.API.Drive.Models.Requests;
using Integrations.Google.Factory;
using File = Google.Apis.Drive.v3.Data.File;

namespace Integrations.Google.API.Drive.Services;

internal sealed partial class GoogleDriveService : IGoogleDriveService
{
    private readonly IGoogleServiceFactory _googleServiceFactory;

    public GoogleDriveService(IGoogleServiceFactory googleServiceFactory)
    {
        _googleServiceFactory = googleServiceFactory;
    }

    public async Task<DriveFile> CreateFolderAsync(
        int userId,
        CreateFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Nazwa folderu nie może być pusta.",
                nameof(request));
        }

        var service = await _googleServiceFactory.CreateDriveAsync(
            userId,
            cancellationToken);

        var folderMetadata = new File
        {
            Name = request.Name,
            Description = request.Description,
            MimeType = "application/vnd.google-apps.folder"
        };

        if (!string.IsNullOrWhiteSpace(request.ParentFolderId))
        {
            folderMetadata.Parents =
            [
                request.ParentFolderId
            ];
        }

        var createRequest = service.Files.Create(folderMetadata);

        createRequest.Fields =
            "id, name, description, mimeType, createdTime, modifiedTime, " +
            "webViewLink, parents, trashed, iconLink, thumbnailLink";

        var folder = await createRequest.ExecuteAsync(cancellationToken);

        return GoogleDriveMapper.Map(folder);
    }

    public async Task DeleteFileAsync(
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

        var service = await _googleServiceFactory.CreateDriveAsync(
            userId,
            cancellationToken);

        var updateMetadata = new File
        {
            Trashed = true
        };

        var updateRequest = service.Files.Update(
            updateMetadata,
            fileId);

        updateRequest.Fields = "id, trashed";

        await updateRequest.ExecuteAsync(cancellationToken);
    }
}