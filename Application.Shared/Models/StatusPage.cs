using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Shared.Models;

public class StatusPage : BaseModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Display settings
    [MaxLength(7)]
    public string? ThemeColor { get; set; } = "#3B82F6";

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(2000)]
    public string? HeaderMessage { get; set; }

    [MaxLength(2000)]
    public string? FooterMessage { get; set; }

    // Auto-refresh interval in seconds
    public int RefreshIntervalSeconds { get; set; } = 30;

    public bool ShowUptime { get; set; } = true;
    public bool ShowResponseTime { get; set; } = true;
    public bool ShowDependencies { get; set; } = true;

    // JSON configuration for layout and display options
    [MaxLength(4000)]
    public string? DisplayConfig { get; set; }

    // Navigation properties
    public virtual ICollection<StatusPageEntity> StatusPageEntities { get; set; } = new List<StatusPageEntity>();
}

// Junction table for many-to-many relationship between StatusPage and Entity
public class StatusPageEntity : BaseModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string StatusPageId { get; set; } = string.Empty;

    [Required]
    public string EntityId { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    public bool IsVisible { get; set; } = true;

    // Group entities together (e.g., "Databases", "Reports", etc.)
    [MaxLength(100)]
    public string? GroupName { get; set; }

    [ForeignKey(nameof(StatusPageId))]
    public virtual StatusPage StatusPage { get; set; } = null!;

    [ForeignKey(nameof(EntityId))]
    public virtual Entity Entity { get; set; } = null!;
}
