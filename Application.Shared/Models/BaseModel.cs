using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Models;

public class BaseModel
{
    public string? WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime? CreatedOn { get; set; } = DateTime.Now;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime? ModifiedOn { get; set; } = DateTime.Now;

    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }

    // Soft delete properties
    public bool IsDeleted { get; set; } = false;
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Aliases for service compatibility
    [NotMapped]
    public DateTime? CreatedAt
    {
        get => CreatedOn;
        set => CreatedOn = value;
    }

    [NotMapped]
    public DateTime? UpdatedAt
    {
        get => ModifiedOn;
        set => ModifiedOn = value;
    }

    [NotMapped]
    public string? UpdatedBy
    {
        get => ModifiedBy;
        set => ModifiedBy = value;
    }

    [NotMapped]
    public bool? IsSelected { get; set; }
}
