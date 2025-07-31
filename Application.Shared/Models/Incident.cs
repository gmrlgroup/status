using System.ComponentModel.DataAnnotations;
using Application.Shared.Enums;

namespace Application.Shared.Models;

public class Incident : BaseModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string EntityId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public IncidentSeverity Severity { get; set; }

    [Required]
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;

    public DateTime? StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    [MaxLength(200)]
    public string? ReportedBy { get; set; }

    [MaxLength(200)]
    public string? AssignedTo { get; set; }

    // Impact details
    [MaxLength(1000)]
    public string? ImpactDescription { get; set; }

    // Resolution details
    [MaxLength(2000)]
    public string? ResolutionDetails { get; set; }

    // External incident tracking ID (e.g., from external systems)
    [MaxLength(100)]
    public string? ExternalIncidentId { get; set; }

    // JSON field for additional metadata
    [MaxLength(4000)]
    public string? Metadata { get; set; }

    // Navigation property back to Entity
    public virtual Entity? Entity { get; set; } = null!;

    // Navigation property for incident updates
    public virtual ICollection<IncidentUpdate>? Updates { get; set; } = new List<IncidentUpdate>();

    // Computed properties
    public TimeSpan? Duration => ResolvedAt.HasValue ? ResolvedAt.Value - StartedAt : DateTime.UtcNow - StartedAt;

    public bool IsResolved => Status == IncidentStatus.Resolved && ResolvedAt.HasValue;

    public string GetSeverityClass()
    {
        return Severity switch
        {
            IncidentSeverity.Low => "bg-blue-100 text-blue-800",
            IncidentSeverity.Medium => "bg-yellow-100 text-yellow-800",
            IncidentSeverity.High => "bg-orange-100 text-orange-800",
            IncidentSeverity.Critical => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }

    public string GetStatusClass()
    {
        return Status switch
        {
            IncidentStatus.Open => "bg-red-100 text-red-800",
            IncidentStatus.Investigating => "bg-yellow-100 text-yellow-800",
            IncidentStatus.Identified => "bg-blue-100 text-blue-800",
            IncidentStatus.Monitoring => "bg-purple-100 text-purple-800",
            IncidentStatus.Resolved => "bg-green-100 text-green-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }

    public string GetSeverityIcon()
    {
        return Severity switch
        {
            IncidentSeverity.Low => "🔵",
            IncidentSeverity.Medium => "🟡",
            IncidentSeverity.High => "🟠",
            IncidentSeverity.Critical => "🔴",
            _ => "⚪"
        };
    }
}
