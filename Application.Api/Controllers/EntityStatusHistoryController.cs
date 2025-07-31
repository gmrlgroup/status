using Microsoft.AspNetCore.Mvc;
using Application.Shared.Services;
using Application.Shared.Models;
using Application.Shared.Enums;
using Application.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Application.Api.Controllers;

/// <summary>
/// Public API controller for external services to manage entity status history
/// </summary>
[ApiController]
[Route("api/status-history")]
[Produces("application/json")]
public class EntityStatusHistoryController : ControllerBase
{
    private readonly IEntityStatusHistoryService _entityStatusHistoryService;
    private readonly IEntityService _entityService;
    private readonly ILogger<EntityStatusHistoryController> _logger;

    public EntityStatusHistoryController(
        IEntityStatusHistoryService entityStatusHistoryService,
        IEntityService entityService,
        ILogger<EntityStatusHistoryController> logger)
    {
        _entityStatusHistoryService = entityStatusHistoryService ?? throw new ArgumentNullException(nameof(entityStatusHistoryService));
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new entity status history record
    /// </summary>
    /// <param name="request">The status history creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created status history record</returns>
    /// <response code="201">Status history record was created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Entity not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    //[ProducesResponseType(typeof(EntityStatusHistoryResponse), StatusCodes.Status201Created)]
    //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateEntityStatusHistory(
        EntityStatusHistory request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating entity status history for entity {EntityId} with status {Status}", 
                request.EntityId, request.Status);

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

                _logger.LogWarning("Entity status history creation failed due to validation errors: {Errors}", 
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

                _logger.LogWarning("Entity {EntityId} not found when creating status history", request.EntityId);
                return NotFound(errorResponse);
            }

            // Create the status history model
            var statusHistory = new EntityStatusHistory
            {
                EntityId = request.EntityId,
                Status = request.Status,
                StatusMessage = request.StatusMessage,
                ResponseTime = request.ResponseTime,
                UptimePercentage = request.UptimePercentage,
                CheckedAt = request.CheckedAt ?? DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "API",
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = "API",
                WorkspaceId = entity.WorkspaceId // Inherit workspace from entity
            };

            // Create the status history using the service
            var createdStatusHistory = await _entityStatusHistoryService.CreateEntityStatusHistoryAsync(statusHistory);


            // Create response
            var response = new EntityStatusHistoryResponse
            {
                Id = createdStatusHistory.Id ?? 0,
                EntityId = createdStatusHistory.EntityId,
                Status = createdStatusHistory.Status,
                StatusMessage = createdStatusHistory.StatusMessage,
                ResponseTime = createdStatusHistory.ResponseTime,
                UptimePercentage = createdStatusHistory.UptimePercentage,
                CheckedAt = createdStatusHistory.CheckedAt,
                CreatedAt = createdStatusHistory.CreatedOn ?? DateTime.UtcNow,
                UpdatedAt = createdStatusHistory.ModifiedOn
            };

            _logger.LogInformation("Successfully created entity status history {Id} for entity {EntityId}", 
                createdStatusHistory.Id, request.EntityId);

            return CreatedAtAction(
                nameof(GetEntityStatusHistory),
                new { id = createdStatusHistory.Id },
                response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity status history for entity {EntityId}", request.EntityId);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while creating the entity status history",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Gets an entity status history record by ID
    /// </summary>
    /// <param name="id">The status history record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The status history record</returns>
    /// <response code="200">Status history record found</response>
    /// <response code="404">Status history record not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EntityStatusHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEntityStatusHistory(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving entity status history {Id}", id);

            var statusHistory = await _entityStatusHistoryService.GetEntityStatusHistoryByIdAsync(id);
            if (statusHistory == null)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"Entity status history with ID '{id}' not found"
                };

                _logger.LogWarning("Entity status history {Id} not found", id);
                return NotFound(errorResponse);
            }

            var response = new EntityStatusHistoryResponse
            {
                Id = statusHistory.Id ?? 0,
                EntityId = statusHistory.EntityId,
                Status = statusHistory.Status,
                StatusMessage = statusHistory.StatusMessage,
                ResponseTime = statusHistory.ResponseTime,
                UptimePercentage = statusHistory.UptimePercentage,
                CheckedAt = statusHistory.CheckedAt,
                CreatedAt = statusHistory.CreatedOn ?? DateTime.UtcNow,
                UpdatedAt = statusHistory.ModifiedOn
            };

            _logger.LogInformation("Successfully retrieved entity status history {Id}", id);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity status history {Id}", id);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while retrieving the entity status history",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
    
}
