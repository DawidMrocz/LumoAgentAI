using Lumo.Api.Attributes.Validation;

namespace Lumo.Api.ApiModels.User
{
    [TrimAllStrings]
    public class DeleteAccountRequest
    {
        /// <summary>
        /// Wymagane dla kont posiadających hasło. Konta wyłącznie społecznościowe mogą pominąć.
        /// </summary>
        public string? CurrentPassword { get; set; }
    }
}
