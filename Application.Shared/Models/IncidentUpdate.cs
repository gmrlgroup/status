using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Application.Shared.Enums;

namespace Application.Shared.Models;

public class IncidentUpdate : BaseModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string IncidentId { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public IncidentStatus? StatusChange { get; set; }

    [MaxLength(200)]
    public string? Author { get; set; }

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;

    // Navigation property back to Incident
    [JsonIgnore]
    public virtual Incident? Incident { get; set; } = null!;
}
