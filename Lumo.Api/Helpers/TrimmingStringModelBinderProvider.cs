using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lumo.Api.Helpers
{
    /// <summary>
    /// Przycina białe znaki we WSZYSTKICH wiązanych stringach jeszcze przed walidacją.
    /// Dzięki temu wartość "   " nie przechodzi już [Required] tylko po to,
    /// żeby chwilę później zostać przyciętą do pustego stringa.
    /// </summary>
    public sealed class TrimmingStringModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Metadata.ModelType == typeof(string)
                ? new TrimmingStringModelBinder()
                : null;
        }
    }

    public sealed class TrimmingStringModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (result == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);

            var value = result.FirstValue?.Trim();

            bindingContext.Result = ModelBindingResult.Success(value);

            return Task.CompletedTask;
        }
    }
}
