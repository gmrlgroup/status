using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Shared.Enums;

namespace Application.Shared.Models;

public class AlertRule : BaseModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Entity this alert applies to (optional for global alerts)
    public string? EntityId { get; set; }

    [ForeignKey(nameof(EntityId))]
    public virtual Entity? Entity { get; set; }

    // Alert conditions (JSON configuration)
    [Required]
    [MaxLength(4000)]
    public string Conditions { get; set; } = string.Empty;

    // Alert severity
    [Required]
    public AlertSeverity Severity { get; set; }

    public bool IsActive { get; set; } = true;

    // Notification settings
    public bool SendEmail { get; set; } = true;
    public bool SendSms { get; set; } = false;
    public bool SendWebhook { get; set; } = false;

    [MaxLength(1000)]
    public string? EmailRecipients { get; set; }

    [MaxLength(500)]
    public string? SmsRecipients { get; set; }

    [MaxLength(500)]
    public string? WebhookUrl { get; set; }

    // Cooldown period in minutes to prevent spam
    public int CooldownMinutes { get; set; } = 15;

    public DateTime? LastTriggered { get; set; }

    // Navigation properties
    public virtual ICollection<AlertInstance> AlertInstances { get; set; } = new List<AlertInstance>();
}

public enum AlertSeverity
{
    [Display(Name = "Low")]
    Low = 1,
    
    [Display(Name = "Medium")]
    Medium = 2,
    
    [Display(Name = "High")]
    High = 3,
    
    [Display(Name = "Critical")]
    Critical = 4
}
