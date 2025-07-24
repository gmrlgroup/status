using System.ComponentModel.DataAnnotations;
using Application.Shared.Models;

namespace Application.Shared.Models.User;

public class AddUserToWorkspaceModel
{
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workspace ID is required")]
    [MaxLength(10)]
    public string WorkspaceId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Member role is required")]
    public MemberRole Role { get; set; } = MemberRole.Member;

    [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
    public string? Notes { get; set; }
}

public class AddUserToWorkspaceByEmailModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workspace ID is required")]
    [MaxLength(10)]
    public string WorkspaceId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Member role is required")]
    public MemberRole Role { get; set; } = MemberRole.Member;

    [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
    public string? Notes { get; set; }
}
