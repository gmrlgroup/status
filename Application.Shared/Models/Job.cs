using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Shared.Enums;

namespace Application.Shared.Models;

public class Job : BaseModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public JobType JobType { get; set; }

    [Required]
    public TriggerType TriggerType { get; set; }

    [Required]
    public JobStatus Status { get; set; } = JobStatus.Pending;

    // Entity this job monitors (optional, some jobs might be global)
    public string? EntityId { get; set; }

    [ForeignKey(nameof(EntityId))]
    public virtual Entity? Entity { get; set; }

    // Cron expression for scheduled jobs
    [MaxLength(100)]
    public string? CronExpression { get; set; }

    // Sensor configuration (JSON)
    [MaxLength(2000)]
    public string? SensorConfig { get; set; }

    // Command or script to execute
    [MaxLength(4000)]
    public string? Command { get; set; }

    // Timeout in seconds
    public int TimeoutSeconds { get; set; } = 300;

    // Retry configuration
    public int MaxRetries { get; set; } = 3;
    public int RetryIntervalSeconds { get; set; } = 60;

    public bool IsActive { get; set; } = true;

    public DateTime? NextRunTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public DateTime? LastSuccessTime { get; set; }

    [MaxLength(2000)]
    public string? LastResult { get; set; }

    [MaxLength(4000)]
    public string? LastError { get; set; }

    // Success rate percentage
    public double? SuccessRate { get; set; }

    // Average execution time in seconds
    public double? AverageExecutionTime { get; set; }

    // Navigation properties
    public virtual ICollection<JobExecution> JobExecutions { get; set; } = new List<JobExecution>();
}
