namespace Labverse.BLL.Interfaces;

public record KnowledgeImportResult(string Url, int VectorId);

public interface IKnowledgeImportService
{
    Task<KnowledgeImportResult> ImportAsync(Stream fileStream, string fileName, string contentType, string? metadata, CancellationToken ct = default);
}
