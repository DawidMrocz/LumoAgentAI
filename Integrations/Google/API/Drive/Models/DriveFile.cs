namespace Integrations.Google.API.Drive.Models;

public sealed class DriveFile
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? MimeType { get; init; }
    public long? Size { get; init; }
    public DateTimeOffset? CreatedTime { get; init; }
    public DateTimeOffset? ModifiedTime { get; init; }
    public string? WebViewLink { get; init; }
    public string? WebContentLink { get; init; }
    public IReadOnlyList<string>? Parents { get; init; }
    public bool? Trashed { get; init; }
    public string? IconLink { get; init; }
    public string? ThumbnailLink { get; init; }
}
