namespace Labverse.BLL.Interfaces;

public record KnowledgeResult(int Id, string Content, double Score, string? Metadata);

public interface IVectorService
{
    Task<int> InsertAsync(
        string content,
        float[] embedding,
        string? metadata = null,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<KnowledgeResult>> GetSimilarAsync(
        float[] embedding,
        int k = 10,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<KnowledgeResult>> SearchAsync(
        float[] embedding,
        int limit = 5,
        double minScore = 0.0,
        CancellationToken ct = default
    );
}
