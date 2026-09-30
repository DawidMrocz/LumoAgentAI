using Lumo.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class FacebookLoginDto
    {
        [Required(ErrorMessage = "AccessToken jest wymagany")]
        public string AccessToken { get; set; } = null!;
    }
}
