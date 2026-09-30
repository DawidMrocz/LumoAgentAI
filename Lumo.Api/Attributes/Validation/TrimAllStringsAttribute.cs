using System.ComponentModel.DataAnnotations;

namespace Lumo.Api.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class TrimAllStringsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success!;

            System.Reflection.PropertyInfo[] properties = value.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo property in properties)
                if (property.PropertyType == typeof(string) && property.CanRead && property.CanWrite)
                {
                    string? currentValue = property.GetValue(value) as string;
                    if (currentValue is not null)
                    {
                        string trimmedStringValue = currentValue.Trim();
                        property.SetValue(value, trimmedStringValue);
                    }
                }

            return ValidationResult.Success!;
        }
    }
}
