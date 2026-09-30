using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Lumo.Api.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PolishPhoneValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success!;

            string stringValue = value as string
                ?? throw new Exception("Nie udana próba parsowania");

            if (!new Regex(@"^(?:\+48)?\d{9}$").IsMatch(stringValue.Replace("-", "").Replace(" ", "")))
                return new ValidationResult("Format numeru telefonu jest nie poprawny");

            return ValidationResult.Success!;
        }
    }
}
