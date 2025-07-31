using System.ComponentModel.DataAnnotations;
using Application.Shared.Enums;

namespace Application.Shared.Models;

/// <summary>
/// Data transfer object for creating entity status history records via public API
/// </summary>
public class CreateEntityStatusHistoryRequest
{
    /// <summary>
    /// The ID of the entity this status check is for
    /// </summary>
    [Required(ErrorMessage = "EntityId is required")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// The status of the entity at the time of check
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    public EntityStatus Status { get; set; }

    /// <summary>
    /// Optional message describing the status or any issues (max 2000 characters)
    /// </summary>
    [MaxLength(2000, ErrorMessage = "StatusMessage cannot exceed 2000 characters")]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Response time in milliseconds (optional)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "ResponseTime must be a positive number")]
    public double? ResponseTime { get; set; }

    /// <summary>
    /// Uptime percentage at the time of this status check (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "UptimePercentage must be between 0 and 100")]
    public double? UptimePercentage { get; set; }

    /// <summary>
    /// When this status check was performed (optional, defaults to current UTC time)
    /// </summary>
    public DateTime? CheckedAt { get; set; }
}

/// <summary>
/// Data transfer object for updating entity status history records
/// </summary>
public class UpdateEntityStatusHistoryRequest
{
    /// <summary>
    /// The status of the entity
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    public EntityStatus Status { get; set; }

    /// <summary>
    /// Optional message describing the status or any issues (max 2000 characters)
    /// </summary>
    [MaxLength(2000, ErrorMessage = "StatusMessage cannot exceed 2000 characters")]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Response time in milliseconds (optional)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "ResponseTime must be a positive number")]
    public double? ResponseTime { get; set; }

    /// <summary>
    /// Uptime percentage at the time of this status check (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "UptimePercentage must be between 0 and 100")]
    public double? UptimePercentage { get; set; }

    /// <summary>
    /// When this status check was performed
    /// </summary>
    [Required(ErrorMessage = "CheckedAt is required")]
    public DateTime CheckedAt { get; set; }
}

/// <summary>
/// Response object for entity status history records
/// </summary>
public class EntityStatusHistoryResponse
{
    /// <summary>
    /// Unique identifier of the status history record
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Entity ID this status record belongs to
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Status of the entity
    /// </summary>
    public EntityStatus Status { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public double? ResponseTime { get; set; }

    /// <summary>
    /// Uptime percentage
    /// </summary>
    public double? UptimePercentage { get; set; }

    /// <summary>
    /// When this status check was performed
    /// </summary>
    public DateTime? CheckedAt { get; set; }

    /// <summary>
    /// When this record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this record was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request object for querying entity status history with filters
/// </summary>
public class EntityStatusHistoryQueryRequest
{
    /// <summary>
    /// Start date for filtering records (optional)
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// End date for filtering records (optional)
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Filter by specific status (optional)
    /// </summary>
    public EntityStatus? Status { get; set; }

    /// <summary>
    /// Page number for pagination (1-based, default: 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of records per page (default: 50, max: 1000)
    /// </summary>
    [Range(1, 1000, ErrorMessage = "PageSize must be between 1 and 1000")]
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Paginated response for entity status history queries
/// </summary>
public class EntityStatusHistoryPagedResponse
{
    /// <summary>
    /// List of status history records for the current page
    /// </summary>
    public List<EntityStatusHistoryResponse> Items { get; set; } = new();

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of records per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Whether there are more pages after the current one
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there are pages before the current one
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Response object for entity status summary statistics
/// </summary>
public class EntityStatusSummaryResponse
{
    /// <summary>
    /// Entity ID this summary is for
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Count of status records by status type
    /// </summary>
    public Dictionary<EntityStatus, int> StatusCounts { get; set; } = new();

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double? AverageResponseTime { get; set; }

    /// <summary>
    /// Average uptime percentage
    /// </summary>
    public double? AverageUptime { get; set; }

    /// <summary>
    /// Total number of status checks performed
    /// </summary>
    public int TotalChecks { get; set; }

    /// <summary>
    /// Start date for the summary period (if specified)
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// End date for the summary period (if specified)
    /// </summary>
    public DateTime? ToDate { get; set; }
}




/// <summary>
/// Generic API error response
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details (optional)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Validation errors (if applicable)
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
