namespace Lumo.Api.Services.RAG.Chunking
{
    public interface IChunkingService
    {
        IReadOnlyList<string> Split(string text, int maxCharacters = 500);
    }
}
