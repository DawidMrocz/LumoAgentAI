using Lumo.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu email.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [PasswordValidation]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [MaxLength(50, ErrorMessage = "Imię może mieć maksymalnie 50 znaków.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [MaxLength(100, ErrorMessage = "Nazwisko może mieć maksymalnie 100 znaków.")]
        public string LastName { get; set; } = null!;
    }
}
