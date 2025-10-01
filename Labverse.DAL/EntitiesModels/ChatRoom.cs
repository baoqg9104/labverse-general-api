using Labverse.DAL.Commons;

namespace Labverse.DAL.EntitiesModels
{
    public class ChatRoom : BaseEntity
    {
        public string Name { get; set; } = null!;
        public ChatRoomType Type { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ChatRoomUser> ChatRoomUsers { get; set; } = new List<ChatRoomUser>();
    }
}
