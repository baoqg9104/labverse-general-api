namespace Labverse.DAL.EntitiesModels
{
    public class Message : BaseEntity
    {
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;
    }
}
