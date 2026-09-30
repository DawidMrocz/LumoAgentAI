namespace Lumo.Api.Services.RAG.Embeddings
{
    public interface IEmbeddingServiceFactory
    {
        IEmbeddingService Create();
    }
}
