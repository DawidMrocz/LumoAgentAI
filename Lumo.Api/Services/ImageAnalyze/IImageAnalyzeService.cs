namespace Lumo.Api.Services.ImageAnalyze
{
    public interface IImageAnalyzeService
    {
        Task<string> AnalyzeAsync(string attachmentId, string question);
    }
}
