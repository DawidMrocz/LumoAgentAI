using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FileValidationAttribute : ValidationAttribute
    {
        private int MaxFileSizeMb { get; }

        private string[] AllowedExtensions { get; }

        /// <param name="fileSize">Maksymalny rozmiar w MB. 0 = bez limitu.</param>
        /// <param name="allowedExtensons">Rozszerzenia rozdzielone przecinkiem, np. "pdf,png,jpg".</param>
        public FileValidationAttribute(int fileSize = 0, string? allowedExtensons = null)
        {
            MaxFileSizeMb = fileSize;

            AllowedExtensions = string.IsNullOrWhiteSpace(allowedExtensons)
                ? []
                : allowedExtensons
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.TrimStart('.').ToLowerInvariant())
                    .ToArray();
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IFormFile file)
                return new ValidationResult("Nie przesłano pliku.");

            if (file.Length <= 0)
                return new ValidationResult("Przesłany plik jest pusty.");

            // Rozmiar sprawdzamy z metadanych żądania - NIE kopiujemy pliku do pamięci.
            // Wcześniejsza wersja robiła file.CopyTo(new MemoryStream()) przed kontrolą
            // rozmiaru, czyli wielkie pliki alokowały się w całości w RAM zanim
            // zostały odrzucone.
            if (MaxFileSizeMb > 0)
            {
                var maxBytes = (long)MaxFileSizeMb * 1024 * 1024;

                if (file.Length > maxBytes)
                    return new ValidationResult($"Maksymalny rozmiar pliku to {MaxFileSizeMb} MB.");
            }

            if (AllowedExtensions.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName)
                    .TrimStart('.')
                    .ToLowerInvariant();

                // Porównanie dokładne, nie Contains - wcześniej "pdf" przechodziło
                // dla dowolnego rozszerzenia będącego podciągiem listy dozwolonych.
                if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                {
                    return new ValidationResult(
                        $"Niedozwolone rozszerzenie pliku. Dozwolone: {string.Join(", ", AllowedExtensions)}.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
