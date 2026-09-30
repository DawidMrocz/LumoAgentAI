#pragma warning disable OPENAI001

namespace Lumo.Api.Services.CodeInterpreter
{
    public interface ICodeInterpreterService
    {
        Task<CodeInterpreterResult> ExecuteAsync(
            string prompt,
            CancellationToken cancellationToken = default);

        Task<byte[]> DownloadFileAsync(
            string containerId,
            string fileId,
            CancellationToken cancellationToken = default);
    }
}
