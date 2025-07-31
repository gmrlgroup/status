using System.ComponentModel.DataAnnotations;
using Application.Shared.Enums;

namespace Application.Shared.Models;

/// <summary>
/// Data transfer object for creating incidents via public API
/// </summary>
public class CreateIncidentRequest
{
    /// <summary>
    /// The ID of the entity this incident is related to
    /// </summary>
    [Required(ErrorMessage = "EntityId is required")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Short title describing the incident (max 200 characters)
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the incident (max 4000 characters)
    /// </summary>
    [Required(ErrorMessage = "Description is required")]
    [MaxLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Severity level of the incident
    /// </summary>
    [Required(ErrorMessage = "Severity is required")]
    public IncidentSeverity Severity { get; set; }

    /// <summary>
    /// Name or identifier of who is reporting this incident
    /// </summary>
    [MaxLength(200, ErrorMessage = "ReportedBy cannot exceed 200 characters")]
    public string? ReportedBy { get; set; }

    /// <summary>
    /// Name or identifier of who is assigned to handle this incident
    /// </summary>
    [MaxLength(200, ErrorMessage = "AssignedTo cannot exceed 200 characters")]
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Description of the impact this incident is having
    /// </summary>
    [MaxLength(1000, ErrorMessage = "ImpactDescription cannot exceed 1000 characters")]
    public string? ImpactDescription { get; set; }

    /// <summary>
    /// External tracking ID from the reporting system
    /// </summary>
    [MaxLength(100, ErrorMessage = "ExternalIncidentId cannot exceed 100 characters")]
    public string? ExternalIncidentId { get; set; }

    /// <summary>
    /// Additional metadata as JSON string
    /// </summary>
    [MaxLength(4000, ErrorMessage = "Metadata cannot exceed 4000 characters")]
    public string? Metadata { get; set; }

    /// <summary>
    /// When the incident started (optional, defaults to current UTC time)
    /// </summary>
    public DateTime? StartedAt { get; set; }
}

/// <summary>
/// Response object for created incident
/// </summary>
public class CreateIncidentResponse
{
    /// <summary>
    /// Unique identifier of the created incident
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title of the incident
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the incident
    /// </summary>
    public IncidentStatus Status { get; set; }

    /// <summary>
    /// Severity of the incident
    /// </summary>
    public IncidentSeverity Severity { get; set; }

    /// <summary>
    /// When the incident was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the incident started
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// External incident ID if provided
    /// </summary>
    public string? ExternalIncidentId { get; set; }
}

///// <summary>
///// Standard API error response
///// </summary>
//public class ApiErrorResponse
//{
//    /// <summary>
//    /// Error message
//    /// </summary>
//    public string Message { get; set; } = string.Empty;

//    /// <summary>
//    /// Additional error details
//    /// </summary>
//    public string? Details { get; set; }

//    /// <summary>
//    /// Collection of validation errors
//    /// </summary>
//    public Dictionary<string, string[]>? ValidationErrors { get; set; }

//    /// <summary>
//    /// Timestamp when the error occurred
//    /// </summary>
//    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
//}
