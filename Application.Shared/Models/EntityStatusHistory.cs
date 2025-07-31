using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Shared.Enums;

namespace Application.Shared.Models;

public class EntityStatusHistory : BaseModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }

    [Required]
    public string EntityId { get; set; } = string.Empty;

    [Required]
    public EntityStatus Status { get; set; }

    [MaxLength(2000)]
    public string? StatusMessage { get; set; }

    // Response time in milliseconds
    public double? ResponseTime { get; set; }

    // Uptime percentage at the time of this status check
    public double? UptimePercentage { get; set; }

    public DateTime? CheckedAt { get; set; } = DateTime.UtcNow;

    // Navigation property back to Entity
    public virtual Entity? Entity { get; set; } = null!;
}
