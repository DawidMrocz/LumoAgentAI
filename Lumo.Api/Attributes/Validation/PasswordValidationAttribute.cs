using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Lumo.Api.Attributes.Validation
{
    /// <summary>
    /// Walidacja siły hasła. Stosować WYŁĄCZNIE tam, gdzie hasło jest ustawiane
    /// (rejestracja, zmiana, reset, set-password) - nigdy przy logowaniu,
    /// bo zaostrzenie polityki odcięłoby istniejących użytkowników.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public partial class PasswordValidationAttribute : ValidationAttribute
    {
        private const int DefaultMinLength = 8;
        private const int MaxLength = 128;

        [GeneratedRegex(@"\d")]
        private static partial Regex DigitPattern();

        [GeneratedRegex(@"[A-ZĄĆĘŁŃÓŚŹŻ]")]
        private static partial Regex UpperPattern();

        [GeneratedRegex(@"[a-ząćęłńóśźż]")]
        private static partial Regex LowerPattern();

        [GeneratedRegex(@"[^a-zA-Z0-9\s]")]
        private static partial Regex SpecialPattern();

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult("Hasło jest wymagane.");

            var password = value.ToString()!;

            var configuration = validationContext.GetService(typeof(IConfiguration)) as IConfiguration;

            var minLength = ReadInt(configuration, "Password:Lenght", DefaultMinLength);

            if (password.Length < minLength)
                return new ValidationResult($"Hasło musi mieć co najmniej {minLength} znaków.");

            // Górny limit chroni przed DoS na BCrypt (koszt hashowania rośnie z długością).
            if (password.Length > MaxLength)
                return new ValidationResult($"Hasło może mieć maksymalnie {MaxLength} znaków.");

            if (ReadBool(configuration, "Password:ContainNumbers", true) && !DigitPattern().IsMatch(password))
                return new ValidationResult("Hasło musi zawierać cyfrę.");

            if (ReadBool(configuration, "Password:ContainBigLetters", true) && !UpperPattern().IsMatch(password))
                return new ValidationResult("Hasło musi zawierać wielką literę.");

            if (ReadBool(configuration, "Password:ContainSmallLetters", true) && !LowerPattern().IsMatch(password))
                return new ValidationResult("Hasło musi zawierać małą literę.");

            if (ReadBool(configuration, "Password:ContainSpecialSigns", true) && !SpecialPattern().IsMatch(password))
                return new ValidationResult("Hasło musi zawierać znak specjalny.");

            return ValidationResult.Success;
        }

        // Błędna wartość w konfiguracji nie może wywracać walidacji wyjątkiem -
        // wcześniej int.Parse/bool.Parse rzucały i kończyły się 500.
        private static int ReadInt(IConfiguration? configuration, string key, int fallback)
        {
            var raw = configuration?[key];

            return int.TryParse(raw, out var parsed) && parsed > 0 ? parsed : fallback;
        }

        private static bool ReadBool(IConfiguration? configuration, string key, bool fallback)
        {
            var raw = configuration?[key];

            return bool.TryParse(raw, out var parsed) ? parsed : fallback;
        }
    }
}
