using Integrations.Google.API.Drive.Models;
using Integrations.Google.API.Drive.Models.Requests;
using System.ComponentModel;

namespace Integrations.Google.API.Drive
{
    public interface IGoogleDriveService
    {
        [Description("Pobiera listę plików i folderów z Google Drive użytkownika. Można określić folder nadrzędny oraz liczbę wyników.")]
        Task<IReadOnlyList<DriveFile>> ListFilesAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Id folderu nadrzędnego. Jeśli null, pobiera pliki z głównego folderu (My Drive).")]
                string? parentFolderId = null,

                [Description("Maksymalna liczba wyników do pobrania. Domyślnie 100.")]
                int maxResults = 100,

                CancellationToken cancellationToken = default
            );

        [Description("Pobiera szczegółowe informacje o konkretnym pliku lub folderze na podstawie jego identyfikatora.")]
        Task<DriveFile?> GetFileAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Unikalny identyfikator pliku lub folderu w Google Drive.")]
                string fileId,

                CancellationToken cancellationToken = default
            );

        [Description("Wyszukuje pliki i foldery w Google Drive na podstawie zapytania tekstowego. Przeszukuje nazwę, opis i zawartość plików.")]
        Task<IReadOnlyList<DriveFile>> SearchFilesAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Tekst wyszukiwania, np. nazwa pliku, słowo kluczowe lub fragment opisu.")]
                string query,

                [Description("Maksymalna liczba wyników do pobrania. Domyślnie 50.")]
                int maxResults = 50,

                CancellationToken cancellationToken = default
            );

        [Description("Tworzy nowy folder w Google Drive. Można określić folder nadrzędny, w którym ma zostać utworzony.")]
        Task<DriveFile> CreateFolderAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Dane nowego folderu - nazwa, opis i opcjonalnie folder nadrzędny.")]
                CreateFolderRequest request,

                CancellationToken cancellationToken = default
            );

        [Description("Usuwa plik lub folder z Google Drive. Plik zostanie przeniesiony do kosza.")]
        Task DeleteFileAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Unikalny identyfikator pliku lub folderu do usunięcia.")]
                string fileId,

                CancellationToken cancellationToken = default
            );
    }
}
