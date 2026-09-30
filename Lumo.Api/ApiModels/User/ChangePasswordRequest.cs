using Lumo.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Obecne hasło jest wymagane")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Nowe hasło jest wymagane")]
        [PasswordValidation]
        public string NewPassword { get; set; } = null!;
    }
}
