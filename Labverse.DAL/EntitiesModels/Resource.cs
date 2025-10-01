namespace Labverse.DAL.EntitiesModels
{
    public class Resource : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int UploadedById { get; set; }
        public User UploadedBy { get; set; } = null!;
    }
}
