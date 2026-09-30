namespace Lumo.Api.Services.Voice
{
    public interface IVoiceService
    {
        Task<string> SpeechToTextAsync(
            byte[] audio,
            CancellationToken cancellationToken = default);

        Task<byte[]> TextToSpeechAsync(
            string text,
            CancellationToken cancellationToken = default);
    }
}
