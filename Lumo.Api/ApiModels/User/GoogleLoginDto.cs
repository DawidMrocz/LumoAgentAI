using Lumo.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class GoogleLoginDto
    {
        [Required(ErrorMessage = "IdToken jest wymagany")]
        public string IdToken { get; set; } = null!;
    }
}
