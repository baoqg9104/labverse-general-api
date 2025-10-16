using Labverse.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Labverse.BLL.Services;

// VectorService encapsulates Supabase vector DB operations (insert + semantic search)
public class VectorService : IVectorService
{
    private readonly HttpClient _http;
    private readonly string _supabaseUrl;
    private readonly string _serviceKey;

    public VectorService(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("supabase");
        _supabaseUrl =
            config["SUPABASE_URL"] ?? throw new InvalidOperationException("SUPABASE_URL missing");
        _serviceKey =
            config["SUPABASE_SERVICE_ROLE_KEY"]
            ?? throw new InvalidOperationException("SUPABASE_SERVICE_ROLE_KEY missing");
    }

    // Inserts a knowledge row with embedding (pgvector) and optional jsonb metadata
    public async Task<int> InsertAsync(
        string content,
        float[] embedding,
        string? metadata = null,
        CancellationToken ct = default
    )
    {
        var url = $"{_supabaseUrl}/rest/v1/knowledge_vectors";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("apikey", _serviceKey);
        req.Headers.Add("Authorization", $"Bearer {_serviceKey}");
        req.Headers.Add("Prefer", "return=representation");
        JsonNode? metaNode = null;
        if (!string.IsNullOrWhiteSpace(metadata))
        {
            try
            {
                metaNode = JsonNode.Parse(metadata);
            }
            catch
            {
                metaNode = null;
            }
        }
        // Convert to double[] to ensure PostgREST/pgvector coercion compatibility
        var embeddingDoubles = Array.ConvertAll(embedding, x => (double)x);
        var body = new
        {
            content,
            embedding = embeddingDoubles,
            metadata = metaNode,
        };
        req.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"
        );
        using var res = await _http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Supabase insert failed: {(int)res.StatusCode} {err}"
            );
        }
        var stream = await res.Content.ReadAsStreamAsync(ct);
        try
        {
            var arr = await JsonSerializer.DeserializeAsync<JsonElement>(
                stream,
                cancellationToken: ct
            );
            if (arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0)
            {
                var id = arr[0].TryGetProperty("id", out var idEl) ? idEl.GetInt32() : 0;
                return id;
            }
        }
        catch { }
        return 0;
    }

    // Executes RPC match_knowledge to fetch top matches by cosine similarity
    public async Task<IReadOnlyList<KnowledgeResult>> SearchAsync(
        float[] embedding,
        int limit = 5,
        double minScore = 0.0,
        CancellationToken ct = default
    )
    {
        var url = $"{_supabaseUrl}/rest/v1/rpc/match_knowledge"; // ensure /rest/v1 prefix
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("apikey", _serviceKey);
        req.Headers.Add("Authorization", $"Bearer {_serviceKey}");
        var payload = new
        {
            query_embedding = embedding,
            match_count = limit,
            min_score = minScore,
        };
        req.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        var stream = await res.Content.ReadAsStreamAsync(ct);
        var json = await JsonSerializer.DeserializeAsync<JsonElement>(
            stream,
            cancellationToken: ct
        );
        var list = new List<KnowledgeResult>();
        if (json.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in json.EnumerateArray())
            {
                int id =
                    item.TryGetProperty("id", out var idEl)
                    && idEl.ValueKind == JsonValueKind.Number
                        ? idEl.GetInt32()
                        : 0;
                string contentStr = item.TryGetProperty("content", out var cEl)
                    ? (
                        cEl.ValueKind == JsonValueKind.String
                            ? cEl.GetString() ?? string.Empty
                            : cEl.ToString()
                    )
                    : string.Empty;
                string? metadataStr = item.TryGetProperty("metadata", out var mEl)
                    ? (mEl.ValueKind == JsonValueKind.String ? mEl.GetString() : mEl.ToString())
                    : null;
                double score = 0.0;
                if (
                    item.TryGetProperty("score", out var sEl)
                    && sEl.ValueKind == JsonValueKind.Number
                )
                {
                    sEl.TryGetDouble(out score);
                }
                list.Add(new KnowledgeResult(id, contentStr, score, metadataStr));
            }
        }
        return list;
    }

    // Low-level nearest neighbors (pre-ranking). Uses get_similar before match_knowledge.
    public async Task<IReadOnlyList<KnowledgeResult>> GetSimilarAsync(
        float[] embedding,
        int k = 10,
        CancellationToken ct = default
    )
    {
        var url = $"{_supabaseUrl}/rest/v1/rpc/get_similar";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("apikey", _serviceKey);
        req.Headers.Add("Authorization", $"Bearer {_serviceKey}");
        var payload = new { query = embedding, k };
        req.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        using var res = await _http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            return Array.Empty<KnowledgeResult>();
        }
        var stream = await res.Content.ReadAsStreamAsync(ct);
        var json = await JsonSerializer.DeserializeAsync<JsonElement>(
            stream,
            cancellationToken: ct
        );
        var list = new List<KnowledgeResult>();
        if (json.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in json.EnumerateArray())
            {
                int id =
                    item.TryGetProperty("id", out var idEl)
                    && idEl.ValueKind == JsonValueKind.Number
                        ? idEl.GetInt32()
                        : 0;
                string contentStr = item.TryGetProperty("content", out var cEl)
                    ? (
                        cEl.ValueKind == JsonValueKind.String
                            ? cEl.GetString() ?? string.Empty
                            : cEl.ToString()
                    )
                    : string.Empty;
                string? metadataStr = item.TryGetProperty("metadata", out var mEl)
                    ? (mEl.ValueKind == JsonValueKind.String ? mEl.GetString() : mEl.ToString())
                    : null;
                list.Add(new KnowledgeResult(id, contentStr, 0.0, metadataStr));
            }
        }
        return list;
    }
}
