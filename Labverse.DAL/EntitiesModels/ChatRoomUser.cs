namespace Labverse.DAL.EntitiesModels
{
    public class ChatRoomUser : BaseEntity
    {   
        public ChatRoom ChatRoom { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
