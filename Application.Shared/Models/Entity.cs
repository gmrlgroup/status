using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Application.Shared.Enums;

namespace Application.Shared.Models;

public class Entity : BaseModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public EntityType EntityType { get; set; }

    [MaxLength(500)]
    public string? Url { get; set; }

    [MaxLength(100)]
    public string? Version { get; set; }

    [MaxLength(200)]
    public string? Owner { get; set; }

    [MaxLength(500)]
    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCritical { get; set; } = false;
    public string? Group { get; set; } = "Default";

    // JSON field for additional metadata
    [MaxLength(4000)]
    public string? Metadata { get; set; }

    // Navigation properties for dependencies
    public virtual ICollection<EntityDependency>? Dependencies { get; set; } = new List<EntityDependency>();
    public virtual ICollection<EntityDependency>? DependentOn { get; set; } = new List<EntityDependency>();

    // Navigation properties for jobs
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    // Navigation property for status history
    public virtual ICollection<EntityStatusHistory> StatusHistory { get; set; } = new List<EntityStatusHistory>();

    // Navigation property for incidents
    [JsonIgnore]
    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();




    public string GetEntityTypeClass()
    {
        return this.EntityType switch
        {
            EntityType.Server => "bg-green-100 text-green-800",
            EntityType.Database => "bg-blue-100 text-blue-800",
            EntityType.Report => "bg-purple-100 text-purple-800",
            EntityType.Dataset => "bg-yellow-100 text-yellow-800",
            EntityType.DataPipeline => "bg-indigo-100 text-indigo-800",
            EntityType.Table => "bg-orange-100 text-orange-800",
            EntityType.DataJob => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }

    public string GetEntityTypeIcon()
    {
        return this.EntityType switch
        {
            EntityType.Server => "🖥️",
            EntityType.Database => "🗄️",
            EntityType.Report => "📊",
            EntityType.Dataset => "📈",
            EntityType.DataPipeline => "🔄",
            EntityType.Table => "📋",
            EntityType.DataJob => "⚙️",
            _ => "📁"
        };
    }
}
