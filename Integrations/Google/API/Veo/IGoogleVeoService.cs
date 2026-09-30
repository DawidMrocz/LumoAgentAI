using Integrations.Google.API.Veo.Models;

namespace Integrations.Google.API.Veo;

public interface IGoogleVeoService
{
    Task<GenerateVideoResponse> GenerateVideoAsync(
        GenerateVideoRequest request,
        int userId,
        CancellationToken cancellationToken = default);

    Task<GenerateVideoResponse> GenerateVideoFromPresetAsync(
        string presetName,
        string prompt,
        int userId,
        CancellationToken cancellationToken = default);

    Task<GenerateVideoResponse> GenerateVideoFromImageAsync(
        ImageToVideoRequest request,
        int userId,
        CancellationToken cancellationToken = default);

    Task<GenerateVideoResponse> GenerateVideoFromMultipleImagesAsync(
        MultiImageToVideoRequest request,
        int userId,
        CancellationToken cancellationToken = default);

    Task<GenerateVideoResponse> ExtendVideoAsync(
        ExtendVideoRequest request,
        int userId,
        CancellationToken cancellationToken = default);

    Task<GenerateVideoResponse> TransferStyleAsync(
        StyleTransferRequest request,
        int userId,
        CancellationToken cancellationToken = default);

    Task<GenerateVideoResponse> GetGenerationStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default);

    Task<List<VeoVideoInfo>> GetUserVideosAsync(
        int userId,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);

    Task<VeoStatistics> GetStatisticsAsync(
        int userId,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default);

    List<VeoPreset> GetPresets();

    Task<bool> CancelGenerationAsync(
        string jobId,
        CancellationToken cancellationToken = default);

    Task<VeoVideoInfo?> GetVideoDetailsAsync(
        int userId,
        int videoId,
        CancellationToken cancellationToken = default);
}
