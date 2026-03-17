namespace Infrastructure.Search.Embedding;

public interface IEmbeddingClient
{
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default);
    Task<float[][]> GetEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken ct = default);
}
