using Integrations.Google.API.Veo.Client.Models;

namespace Integrations.Google.API.Veo.Client;

internal interface IGoogleVeoClient
{
    Task<string?> PredictAsync(
        VeoApiRequest request,
        CancellationToken cancellationToken = default);

    Task<VeoLongRunningResponse?> GetOperationAsync(
        string operationName,
        CancellationToken cancellationToken = default);

    Task<byte[]> DownloadVideoAsync(
        string videoUri,
        CancellationToken cancellationToken = default);
}
