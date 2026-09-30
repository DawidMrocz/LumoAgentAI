using Integrations.Google.API.Drive;
using Integrations.Google.API.Drive.Models.Requests;
using Lumo.Api.Services.AgentUserContext;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Lumo.Api.Services.AgentTools.Google;

/// <summary>
/// Provider narzędzi Google Drive dla agenta AI
/// </summary>
public sealed class DriveAgentTools : IAgentToolProvider
{
    private readonly IGoogleDriveService _driveService;
    private readonly IAgentUserContextService _userContext;

    public DriveAgentTools(
        IGoogleDriveService driveService,
        IAgentUserContextService userContext)
    {
        _driveService = driveService;
        _userContext = userContext;
    }

    public IEnumerable<AITool> GetTools()
    {
        yield return AIFunctionFactory.Create(
            ListDriveFilesAsync,
            name: "ListDriveFiles",
            description: "Pobiera listę plików i folderów z Google Drive użytkownika.");

        yield return AIFunctionFactory.Create(
            GetDriveFileAsync,
            name: "GetDriveFile",
            description: "Pobiera szczegółowe informacje o konkretnym pliku lub folderze z Google Drive.");

        yield return AIFunctionFactory.Create(
            SearchDriveFilesAsync,
            name: "SearchDriveFiles",
            description: "Wyszukuje pliki i foldery w Google Drive na podstawie zapytania tekstowego.");

        yield return AIFunctionFactory.Create(
            CreateDriveFolderAsync,
            name: "CreateDriveFolder",
            description: "Tworzy nowy folder w Google Drive użytkownika.");

        yield return AIFunctionFactory.Create(
            DeleteDriveFileAsync,
            name: "DeleteDriveFile",
            description: "Usuwa plik lub folder z Google Drive (przenosi do kosza).");
    }

    private async Task<string> ListDriveFilesAsync(
        string? parentFolderId = null,
        int maxResults = 100)
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return "Nie udało się ustalić użytkownika.";
        }

        var files = await _driveService.ListFilesAsync(
            userIdInt,
            parentFolderId,
            maxResults);

        if (files.Count == 0)
        {
            return parentFolderId == null
                ? "Brak plików w głównym folderze Google Drive."
                : $"Brak plików w folderze {parentFolderId}.";
        }

        return JsonSerializer.Serialize(files, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private async Task<string> GetDriveFileAsync(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            return "Nie podano identyfikatora pliku.";
        }

        var userId = _userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return "Nie udało się ustalić użytkownika.";
        }

        var file = await _driveService.GetFileAsync(userIdInt, fileId);

        if (file == null)
        {
            return $"Nie znaleziono pliku o ID: {fileId}";
        }

        return JsonSerializer.Serialize(file, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private async Task<string> SearchDriveFilesAsync(
        string query,
        int maxResults = 50)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Nie podano zapytania wyszukiwania.";
        }

        var userId = _userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return "Nie udało się ustalić użytkownika.";
        }

        var files = await _driveService.SearchFilesAsync(
            userIdInt,
            query,
            maxResults);

        if (files.Count == 0)
        {
            return $"Nie znaleziono plików pasujących do zapytania: {query}";
        }

        return JsonSerializer.Serialize(files, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private async Task<string> CreateDriveFolderAsync(
        string name,
        string? description = null,
        string? parentFolderId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Nie podano nazwy folderu.";
        }

        var userId = _userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return "Nie udało się ustalić użytkownika.";
        }

        var request = new CreateFolderRequest
        {
            Name = name,
            Description = description,
            ParentFolderId = parentFolderId
        };

        var folder = await _driveService.CreateFolderAsync(userIdInt, request);

        return JsonSerializer.Serialize(folder, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private async Task<string> DeleteDriveFileAsync(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            return "Nie podano identyfikatora pliku.";
        }

        var userId = _userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return "Nie udało się ustalić użytkownika.";
        }

        await _driveService.DeleteFileAsync(userIdInt, fileId);

        return $"Plik {fileId} został usunięty (przeniesiony do kosza).";
    }
}
