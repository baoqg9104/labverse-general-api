using Microsoft.AspNetCore.Http;

namespace Labverse.BLL.DTOs.Badge
{
    public class BadgesRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? Icon { get; set; }
    }
}
