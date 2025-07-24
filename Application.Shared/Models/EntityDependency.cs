using Application.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Application.Shared.Models;

public class EntityDependency : BaseModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? Id { get; set; }

    [Required]
    public string EntityId { get; set; } = string.Empty;

    [Required]
    public string DependsOnEntityId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCritical { get; set; } = false;

    public EntityType? DependencyType { get; set; }

    // Order of dependency check (lower numbers checked first)
    public int Order { get; set; } = 0;

    // Navigation properties
    [JsonIgnore]
    [ForeignKey(nameof(EntityId))]
    public virtual Entity? Entity { get; set; } = null!;

    [JsonIgnore]
    [ForeignKey(nameof(DependsOnEntityId))]
    public virtual Entity? DependsOnEntity { get; set; } = null!;
}
