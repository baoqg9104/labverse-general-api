namespace Labverse.API.DTOs.Knowledge;

public class KnowledgeImportForm
{
    public IFormFile File { get; set; } = default!;
    public IFormFile? Metadata { get; set; }
}
