using Labverse.API.DTOs.Knowledge;
using Labverse.API.Helpers;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

[Route("api/knowledge")]
[ApiController]
//[Authorize]
public class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeImportService _import;

    public KnowledgeController(IKnowledgeImportService import)
    {
        _import = import;
    }

    [HttpPost("import")]
    [DisableRequestSizeLimit]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(
        [FromForm] KnowledgeImportForm form,
        CancellationToken ct
    )
    {
        try
        {
            var file = form.File;
            string? metadata = null;
            if (form.Metadata != null && form.Metadata.Length > 0)
            {
                using var ms = new MemoryStream();
                await form.Metadata.CopyToAsync(ms, ct);
                metadata = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
            if (file == null || file.Length == 0)
                return ApiErrorHelper.Error("BAD_REQUEST", "file is required", 400);
            await using var stream = file.OpenReadStream();
            var result = await _import.ImportAsync(
                stream,
                file.FileName,
                file.ContentType,
                metadata,
                ct
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("KNOWLEDGE_IMPORT_ERROR", ex.Message, 500);
        }
    }
}
