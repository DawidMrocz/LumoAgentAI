namespace Lumo.Api.Services.ImageGeneration
{

    public interface IImageGenerationService
    {
        Task<byte[]> GenerateAsync(string prompt, string? size = null, CancellationToken cancellationToken = default);
    }
}
