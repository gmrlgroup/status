using Application.Shared.Models.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Models;

public enum MemberRole
{
    Owner = 1,
    Member = 2,
    Coach = 3,
    Customer = 4
}


[PrimaryKey(nameof(WorkspaceId), nameof(ApplicationUserId))]
public class WorkspaceMember
{
    [MaxLength(10)]
    public string? WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public MemberRole Role { get; set; } = MemberRole.Member;

    public DateTime? JoinedDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;

    [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
    public string? Notes { get; set; }
}
