using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Labs;

public class LabCommentDto
{
    public int Id { get; set; }
    public int LabId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

//public class LabCommentTreeDto : LabCommentDto
//{
//    public List<LabCommentTreeDto> Replies { get; set; } = new();
//    public bool IsDeleted { get; set; }
//}

public class EditCommentRequest
{
    public string Content { get; set; } = string.Empty;
}
