using System.Net.Http.Json;
using System.Text.Json;
using eShopX.Application.Exceptions;

namespace Infrastructure.Search.Embedding;

public class HuggingFaceEmbeddingClient(HttpClient client) : IEmbeddingClient
{
    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default)
    {

        var response = await client.PostAsJsonAsync(string.Empty, new { inputs = text }, ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException("HuggingFace",
                await response.Content.ReadAsStringAsync(ct));

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // bge-m3 returns float[] directly; guard against float[][] shape
        if (root.ValueKind == JsonValueKind.Array && root[0].ValueKind == JsonValueKind.Array)
            return root[0].Deserialize<float[]>()!;

        return root.Deserialize<float[]>()!;
    }
}
