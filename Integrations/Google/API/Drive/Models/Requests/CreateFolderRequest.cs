namespace Integrations.Google.API.Drive.Models.Requests;

public sealed class CreateFolderRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? ParentFolderId { get; init; }
}
