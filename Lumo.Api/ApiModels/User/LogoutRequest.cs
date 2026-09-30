using Lumo.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class LogoutRequest
    {
        [Required(ErrorMessage = "RefreshToken jest wymagany")]
        public string RefreshToken { get; set; } = null!;
    }
}
