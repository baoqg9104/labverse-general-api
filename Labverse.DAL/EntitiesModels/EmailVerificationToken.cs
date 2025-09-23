using System.ComponentModel.DataAnnotations.Schema;

namespace Labverse.DAL.EntitiesModels
{
    public class EmailVerificationToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsUsed { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}