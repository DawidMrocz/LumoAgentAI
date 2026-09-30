using Lumo.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token jest wymagany")]
        public string Token { get; set; } = null!;
        [Required(ErrorMessage = "Nowe hasło jest wymagane")]
        [PasswordValidation]
        public string NewPassword { get; set; } = null!;
    }
}
