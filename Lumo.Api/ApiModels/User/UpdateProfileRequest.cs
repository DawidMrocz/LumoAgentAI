using Lumo.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Imię jest wymagane")]
        [MinLength(2, ErrorMessage = "Imię musi mieć co najmniej 2 znaki")]
        [MaxLength(50, ErrorMessage = "Imię może mieć maksymalnie 50 znaków")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [MinLength(2, ErrorMessage = "Nazwisko musi mieć co najmniej 2 znaki")]
        [MaxLength(100, ErrorMessage = "Nazwisko może mieć maksymalnie 100 znaków")]
        public string LastName { get; set; } = null!;
    }
}
