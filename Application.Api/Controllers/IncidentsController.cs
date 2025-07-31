using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Shared.Services;
using Application.Shared.Models;
using Application.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Api.Controllers;

/// <summary>
/// Public API controller for external services to create and manage incidents
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidentService;
    private readonly IEntityService _entityService;
    private readonly ILogger<IncidentsController> _logger;

    public IncidentsController(
        IIncidentService incidentService,
        IEntityService entityService,
        ILogger<IncidentsController> logger)
    {
        _incidentService = incidentService ?? throw new ArgumentNullException(nameof(incidentService));
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new incident for an entity
    /// </summary>
    /// <param name="request">The incident creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created incident details</returns>
    /// <response code="201">Incident was created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Entity not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    //[ProducesResponseType(typeof(CreateIncidentResponse), StatusCodes.Status201Created)]
    //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateIncident(
        Incident request)//,
        //CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating incident for entity {EntityId} with title '{Title}'", 
                request.EntityId, request.Title);

            // Validate the request
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var errorResponse = new ApiErrorResponse
                {
                    Message = "Validation failed",
                    ValidationErrors = validationErrors
                };

                _logger.LogWarning("Incident creation failed due to validation errors: {Errors}", 
                    string.Join(", ", validationErrors.SelectMany(x => x.Value)));

                return BadRequest(errorResponse);
            }

            // Verify that the entity exists
            var entity = await _entityService.GetEntityAsync(request.EntityId);
            if (entity == null)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"Entity with ID '{request.EntityId}' not found"
                };

                _logger.LogWarning("Entity {EntityId} not found when creating incident", request.EntityId);
                return NotFound(errorResponse);
            }

            // Create the incident model
            var incident = new Incident
            {
                Id = Guid.NewGuid().ToString(),
                EntityId = request.EntityId,
                Title = request.Title,
                Description = request.Description,
                Severity = request.Severity,
                Status = IncidentStatus.Open,
                StartedAt = request.StartedAt ?? DateTime.UtcNow,
                ReportedBy = request.ReportedBy,
                AssignedTo = request.AssignedTo,
                ImpactDescription = request.ImpactDescription,
                ExternalIncidentId = request.ExternalIncidentId,
                WorkspaceId = entity.WorkspaceId,
                Metadata = request.Metadata,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "API", // Since this is a public API, mark as API created
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = "API"
            };

            // Create the incident using the service
            var createdIncident = await _incidentService.CreateIncidentAsync(incident);

            // Create response
            var response = new CreateIncidentResponse
            {
                Id = createdIncident.Id,
                Title = createdIncident.Title,
                Status = createdIncident.Status,
                Severity = createdIncident.Severity,
                CreatedAt = createdIncident.CreatedOn ?? DateTime.UtcNow,
                StartedAt = createdIncident.StartedAt,
                ExternalIncidentId = createdIncident.ExternalIncidentId
            };

            _logger.LogInformation("Successfully created incident {IncidentId} for entity {EntityId}", 
                createdIncident.Id, request.EntityId);

            return CreatedAtAction(
                nameof(GetIncident),
                new { id = createdIncident.Id },
                response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident for entity {EntityId}", request.EntityId);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while creating the incident",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Gets an incident by ID
    /// </summary>
    /// <param name="id">The incident ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The incident details</returns>
    /// <response code="200">Incident found</response>
    /// <response code="404">Incident not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Incident), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetIncident(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving incident {IncidentId}", id);

            var incident = await _incidentService.GetIncidentAsync(id);
            if (incident == null)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"Incident with ID '{id}' not found"
                };

                _logger.LogWarning("Incident {IncidentId} not found", id);
                return NotFound(errorResponse);
            }

            _logger.LogInformation("Successfully retrieved incident {IncidentId}", id);
            return Ok(incident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident {IncidentId}", id);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while retrieving the incident",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Updates the status of an incident
    /// </summary>
    /// <param name="id">The incident ID</param>
    /// <param name="status">The new status</param>
    /// <param name="message">Optional status update message</param>
    /// <param name="updatedBy">Who is updating the status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated incident</returns>
    /// <response code="200">Status updated successfully</response>
    /// <response code="400">Invalid status</response>
    /// <response code="404">Incident not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(Incident), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateIncidentStatus(
        [FromRoute] string id,
        [FromBody, Required] IncidentStatus status,
        [FromQuery] string? message = null,
        [FromQuery] string? updatedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating status of incident {IncidentId} to {Status}", id, status);

            // Validate that the incident exists
            var existingIncident = await _incidentService.GetIncidentAsync(id);
            if (existingIncident == null)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"Incident with ID '{id}' not found"
                };

                _logger.LogWarning("Incident {IncidentId} not found when updating status", id);
                return NotFound(errorResponse);
            }

            // Update the incident status
            var updatedIncident = await _incidentService.UpdateIncidentStatusAsync(
                id, 
                status, 
                message, 
                updatedBy ?? "API");

            _logger.LogInformation("Successfully updated status of incident {IncidentId} to {Status}", 
                id, status);

            return Ok(updatedIncident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status of incident {IncidentId}", id);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while updating the incident status",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Resolves an incident
    /// </summary>
    /// <param name="id">The incident ID</param>
    /// <param name="resolutionDetails">Details about how the incident was resolved</param>
    /// <param name="resolvedBy">Who resolved the incident</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The resolved incident</returns>
    /// <response code="200">Incident resolved successfully</response>
    /// <response code="400">Invalid resolution details</response>
    /// <response code="404">Incident not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}/resolve")]
    [ProducesResponseType(typeof(Incident), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResolveIncident(
        [FromRoute] string id,
        [FromBody, Required] string resolutionDetails,
        [FromQuery] string? resolvedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Resolving incident {IncidentId}", id);

            if (string.IsNullOrWhiteSpace(resolutionDetails))
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = "Resolution details are required"
                };

                return BadRequest(errorResponse);
            }

            // Validate that the incident exists
            var existingIncident = await _incidentService.GetIncidentAsync(id);
            if (existingIncident == null)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"Incident with ID '{id}' not found"
                };

                _logger.LogWarning("Incident {IncidentId} not found when resolving", id);
                return NotFound(errorResponse);
            }

            // Resolve the incident
            var resolvedIncident = await _incidentService.ResolveIncidentAsync(
                id, 
                resolutionDetails, 
                resolvedBy ?? "API");

            _logger.LogInformation("Successfully resolved incident {IncidentId}", id);

            return Ok(resolvedIncident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving incident {IncidentId}", id);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while resolving the incident",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
