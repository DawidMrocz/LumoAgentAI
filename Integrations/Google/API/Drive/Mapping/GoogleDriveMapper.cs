using Integrations.Google.API.Drive.Models;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace Integrations.Google.API.Drive.Mapping;

internal static class GoogleDriveMapper
{
    public static DriveFile Map(GoogleFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        return new DriveFile
        {
            Id = file.Id ?? string.Empty,
            Name = file.Name ?? string.Empty,
            Description = file.Description,
            MimeType = file.MimeType,
            Size = file.Size,
            CreatedTime = file.CreatedTimeDateTimeOffset,
            ModifiedTime = file.ModifiedTimeDateTimeOffset,
            WebViewLink = file.WebViewLink,
            WebContentLink = file.WebContentLink,
            Parents = file.Parents?.ToList(),
            Trashed = file.Trashed,
            IconLink = file.IconLink,
            ThumbnailLink = file.ThumbnailLink
        };
    }
}