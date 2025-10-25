namespace Labverse.BLL.Interfaces
{
    public interface ISupabaseService
    {
        Task<string> UploadBadgeIconAsync(Stream fileStream, string fileName);
    }
}
