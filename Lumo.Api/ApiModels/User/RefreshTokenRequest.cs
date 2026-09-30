using Lumo.Api.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;
    }
}
