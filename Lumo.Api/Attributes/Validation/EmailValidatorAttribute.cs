using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Lumo.Api.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class EmailValidatorAttribute : ValidationAttribute
    {
        public EmailValidatorAttribute() { }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success;

            if (value is not string email)
                return new ValidationResult($"Pole '{validationContext.DisplayName}' musi być tekstem!");

            if (new Regex(@"^(([^<>()[\]\\.,;:\s@""]+(\.[^<>()[\]\\.,;:\s@""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([\p{L}\-0-9]+\.)+[a-zA-Z]{2,}))$").IsMatch(email))
                return ValidationResult.Success;

            return new ValidationResult($"Niepoprawny format dla pola '{validationContext.DisplayName}'!");
        }
    }
}
