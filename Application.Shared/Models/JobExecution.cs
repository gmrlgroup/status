using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Shared.Enums;

namespace Application.Shared.Models;

public class JobExecution : BaseModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string JobId { get; set; } = string.Empty;

    [ForeignKey(nameof(JobId))]
    public virtual Job Job { get; set; } = null!;

    [Required]
    public JobStatus Status { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    // Execution time in seconds
    public double? ExecutionTimeSeconds { get; set; }

    [MaxLength(4000)]
    public string? Result { get; set; }

    [MaxLength(4000)]
    public string? ErrorMessage { get; set; }

    [MaxLength(2000)]
    public string? Output { get; set; }

    // Exit code for command-based jobs
    public int? ExitCode { get; set; }

    // Retry attempt number (0 for first attempt)
    public int RetryAttempt { get; set; } = 0;

    // Triggered by (manual, cron, sensor, etc.)
    [MaxLength(100)]
    public string? TriggeredBy { get; set; }

    // Additional execution metadata (JSON)
    [MaxLength(2000)]
    public string? Metadata { get; set; }
}
