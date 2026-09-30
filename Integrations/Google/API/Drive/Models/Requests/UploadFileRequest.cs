namespace Integrations.Google.API.Drive.Models.Requests;

public sealed class UploadFileRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? MimeType { get; init; }
    public required Stream FileContent { get; init; }
    public string? ParentFolderId { get; init; }
}
