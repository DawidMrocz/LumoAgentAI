using Lumo.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class LoginRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu email.")]
        [MaxLength(320, ErrorMessage = "Adres email jest zbyt długi.")]
        public string Email { get; set; } = null!;

        // Świadomie BEZ [PasswordValidation]: przy logowaniu sprawdzamy hasło,
        // a nie jego zgodność z aktualną polityką. Inaczej zaostrzenie polityki
        // zablokowałoby logowanie wszystkim istniejącym użytkownikom.
        [Required(AllowEmptyStrings = false, ErrorMessage = "Hasło jest wymagane.")]
        [MaxLength(128, ErrorMessage = "Hasło jest zbyt długie.")]
        public string Password { get; set; } = null!;
    }
}
