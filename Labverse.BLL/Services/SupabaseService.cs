using Labverse.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using Supabase;

namespace Labverse.BLL.Services
{
    public class SupabaseService : ISupabaseService
    {
        private readonly Client _supabaseClient;
        private string _bucket = "";
        public SupabaseService(IConfiguration configuration)
        {
            var url = configuration["Supabase:Url"];
            var key = configuration["Supabase:Key"];
            _bucket = configuration["Supabase:TestBucket"];

            _supabaseClient = new Client(url, key);
        }
        public async Task<string> UploadBadgeIconAsync(Stream fileStream, string fileName)
        {
            var storage = _supabaseClient.Storage.From(_bucket);

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            await storage.Upload(bytes, uniqueFileName, new Supabase.Storage.FileOptions
            {
                CacheControl = "3600",
                Upsert = false
            });

            var publicUrl = storage.GetPublicUrl(uniqueFileName);
            return publicUrl;
        }
    }
}
