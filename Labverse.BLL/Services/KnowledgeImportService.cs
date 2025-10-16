using Labverse.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Labverse.BLL.Services;

// KnowledgeImportService uploads files to Supabase Storage, reads text, embeds with Gemini and stores vectors
public class KnowledgeImportService : IKnowledgeImportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IVectorService _vectors;
    private readonly ILogger<KnowledgeImportService> _logger;
    private readonly string _supabaseUrl;
    private readonly string _serviceKey;
    private readonly string _bucket;
    private readonly string _geminiApiKey;
    private readonly int _embeddingDim;

    public KnowledgeImportService(
        IHttpClientFactory httpClientFactory,
        IVectorService vectors,
        IConfiguration config,
        ILogger<KnowledgeImportService> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _vectors = vectors;
        _logger = logger;
        _supabaseUrl =
            config["SUPABASE_URL"] ?? throw new InvalidOperationException("SUPABASE_URL missing");
        _serviceKey =
            config["SUPABASE_SERVICE_ROLE_KEY"]
            ?? throw new InvalidOperationException("SUPABASE_SERVICE_ROLE_KEY missing");
        _bucket = config["SUPABASE_STORAGE_BUCKET"] ?? "knowledge-base";
        _geminiApiKey =
            config["GEMINI_API_KEY"]
            ?? throw new InvalidOperationException("GEMINI_API_KEY missing");
        _embeddingDim = int.TryParse(config["EMBEDDING_DIM"], out var dim) ? dim : 1536;
    }

    // High-level import: upload file, extract text, embed, and insert into vector DB
    public async Task<KnowledgeImportResult> ImportAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? metadata,
        CancellationToken ct = default
    )
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext is not ".txt" and not ".md")
            throw new InvalidOperationException("Unsupported file type. Only .txt and .md");

        var safeName = SanitizeFileName(fileName);
        var storageUrl = await UploadToStorageAsync(fileStream, safeName, contentType, ct);

        var text =
            await ReadTextAsync(storageUrl, ct)
            ?? throw new InvalidOperationException("Empty content");
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Empty content");
        if (text.Length > 50000)
            text = text[..50000]; // keep payload reasonable

        var embedding = await EmbedAsync(text, ct);
        embedding = NormalizeEmbedding(embedding);
        int vectorId = await _vectors.InsertAsync(text, embedding, metadata, ct);
        _logger.LogInformation(
            "Imported knowledge file {File} to {Url} with vector {VectorId}",
            safeName,
            storageUrl,
            vectorId
        );
        return new KnowledgeImportResult(storageUrl, vectorId);
    }

    // Keep filenames safe for Storage paths
    private static string SanitizeFileName(string name)
    {
        name = name.Replace("\\", "/");
        name = Path.GetFileName(name);
        return Regex.Replace(name, "[^a-zA-Z0-9._-]", "_");
    }

    // Upload raw file bytes to Supabase Storage using service role key
    private async Task<string> UploadToStorageAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct
    )
    {
        var client = _httpClientFactory.CreateClient("supabase");
        var objectPath = $"{_bucket}/{Uri.EscapeDataString(fileName)}";
        var url = $"{_supabaseUrl}/storage/v1/object/{objectPath}";
        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StreamContent(stream),
        };
        req.Headers.Add("apikey", _serviceKey);
        req.Headers.Add("Authorization", $"Bearer {_serviceKey}");
        req.Headers.Add("x-upsert", "true");
        var ctValue = string.IsNullOrWhiteSpace(contentType)
            ? (
                Path.GetExtension(fileName).ToLowerInvariant() == ".md"
                    ? "text/markdown"
                    : "text/plain"
            )
            : contentType;
        req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ctValue);
        if (stream.CanSeek)
        {
            stream.Position = 0;
            req.Content.Headers.ContentLength = stream.Length;
        }
        using var res = await client.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Supabase upload failed: {(int)res.StatusCode} {err}"
            );
        }
        var publicUrl = $"{_supabaseUrl}/storage/v1/object/public/{objectPath}";
        return publicUrl;
    }

    // Download public object content as text (supports .md/.txt)
    private async Task<string?> ReadTextAsync(string publicUrl, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        using var res = await client.GetAsync(publicUrl, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync(ct);
    }

    // Call Gemini embeddings to get vector representation of the text
    private async Task<float[]> EmbedAsync(string text, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("gemini");
        var url =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("x-goog-api-key", _geminiApiKey);
        var payload = new
        {
            model = "models/gemini-embedding-001",
            content = new { parts = new object[] { new { text } } },
        };
        req.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        using var res = await client.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        using var stream = await res.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var arr = doc.RootElement.GetProperty("embedding").GetProperty("values");
        var vector = new float[arr.GetArrayLength()];
        var i = 0;
        foreach (var v in arr.EnumerateArray())
            vector[i++] = v.GetSingle();
        return vector;
    }

    // Ensure embedding dimension matches database schema (pad/truncate)
    private float[] NormalizeEmbedding(float[] v)
    {
        if (v.Length == _embeddingDim)
            return v;
        if (v.Length > _embeddingDim)
        {
            var sliced = new float[_embeddingDim];
            Array.Copy(v, sliced, _embeddingDim);
            return sliced;
        }
        var padded = new float[_embeddingDim];
        Array.Copy(v, padded, v.Length);
        return padded;
    }
}
