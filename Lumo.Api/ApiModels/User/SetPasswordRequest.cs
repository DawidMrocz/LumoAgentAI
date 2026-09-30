using Lumo.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class SetPasswordRequest
    {
        [Required(ErrorMessage = "Nowe hasło jest wymagane")]
        [PasswordValidation]
        public string NewPassword { get; set; } = null!;
    }
}
