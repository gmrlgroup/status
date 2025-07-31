using Microsoft.AspNetCore.Mvc;
using Application.Shared.Services;
using Application.Shared.Models;
using Application.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Controllers;

/// <summary>
/// Public API controller for external services to manage entity status history
/// </summary>
[ApiController]
[Route("api/entity-status-history")]
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
    [ProducesResponseType(typeof(EntityStatusHistoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateEntityStatusHistory(
        [FromBody] CreateEntityStatusHistoryRequest request,
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

    /// <summary>
    /// Gets entity status history for a specific entity with optional filtering
    /// </summary>
    /// <param name="entityId">The entity ID</param>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of status history records</returns>
    /// <response code="200">Status history records retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="404">Entity not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("entity/{entityId}")]
    [ProducesResponseType(typeof(EntityStatusHistoryPagedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEntityStatusHistoryByEntity(
        [FromRoute] string entityId,
        [FromQuery] EntityStatusHistoryQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving entity status history for entity {EntityId}", entityId);

            // Validate that the entity exists
            var entity = await _entityService.GetEntityAsync(entityId);
            if (entity == null)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"Entity with ID '{entityId}' not found"
                };

                _logger.LogWarning("Entity {EntityId} not found when retrieving status history", entityId);
                return NotFound(errorResponse);
            }

            // Get paginated results
            var statusHistoryList = await _entityStatusHistoryService.GetEntityStatusHistoryWithPaginationAsync(
                entityId, query.Page, query.PageSize);

            // Filter by status if specified
            if (query.Status.HasValue)
            {
                statusHistoryList = statusHistoryList.Where(h => h.Status == query.Status.Value).ToList();
            }

            // Filter by date range if specified
            if (query.FromDate.HasValue || query.ToDate.HasValue)
            {
                var filteredList = await _entityStatusHistoryService.GetEntityStatusHistoryAsync(
                    entityId, query.FromDate, query.ToDate);
                statusHistoryList = filteredList
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToList();
            }

            // Get total count for pagination
            var totalCount = await _entityStatusHistoryService.GetEntityStatusHistoryCountAsync(entityId);

            // Convert to response objects
            var responseItems = statusHistoryList.Select(h => new EntityStatusHistoryResponse
            {
                Id = h.Id ?? 0,
                EntityId = h.EntityId,
                Status = h.Status,
                StatusMessage = h.StatusMessage,
                ResponseTime = h.ResponseTime,
                UptimePercentage = h.UptimePercentage,
                CheckedAt = h.CheckedAt,
                CreatedAt = h.CreatedOn ?? DateTime.UtcNow,
                UpdatedAt = h.ModifiedOn
            }).ToList();

            var response = new EntityStatusHistoryPagedResponse
            {
                Items = responseItems,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };

            _logger.LogInformation("Successfully retrieved {Count} entity status history records for entity {EntityId}", 
                responseItems.Count, entityId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity status history for entity {EntityId}", entityId);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while retrieving entity status history",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Updates an entity status history record
    /// </summary>
    /// <param name="id">The status history record ID</param>
    /// <param name="request">The update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated status history record</returns>
    /// <response code="200">Status history record updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Status history record not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EntityStatusHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEntityStatusHistory(
        [FromRoute] int id,
        [FromBody] UpdateEntityStatusHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating entity status history {Id}", id);

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

                return BadRequest(errorResponse);
            }

            // Check if the record exists
            var existingRecord = await _entityStatusHistoryService.GetEntityStatusHistoryByIdAsync(id);
            if (existingRecord == null)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"Entity status history with ID '{id}' not found"
                };

                _logger.LogWarning("Entity status history {Id} not found for update", id);
                return NotFound(errorResponse);
            }

            // Update the record
            existingRecord.Status = request.Status;
            existingRecord.StatusMessage = request.StatusMessage;
            existingRecord.ResponseTime = request.ResponseTime;
            existingRecord.UptimePercentage = request.UptimePercentage;
            existingRecord.CheckedAt = request.CheckedAt;
            existingRecord.ModifiedBy = "API";

            var updatedRecord = await _entityStatusHistoryService.UpdateEntityStatusHistoryAsync(existingRecord);

            var response = new EntityStatusHistoryResponse
            {
                Id = updatedRecord.Id ?? 0,
                EntityId = updatedRecord.EntityId,
                Status = updatedRecord.Status,
                StatusMessage = updatedRecord.StatusMessage,
                ResponseTime = updatedRecord.ResponseTime,
                UptimePercentage = updatedRecord.UptimePercentage,
                CheckedAt = updatedRecord.CheckedAt,
                CreatedAt = updatedRecord.CreatedOn ?? DateTime.UtcNow,
                UpdatedAt = updatedRecord.ModifiedOn
            };

            _logger.LogInformation("Successfully updated entity status history {Id}", id);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity status history {Id}", id);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while updating the entity status history",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Deletes an entity status history record
    /// </summary>
    /// <param name="id">The status history record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success confirmation</returns>
    /// <response code="204">Status history record deleted successfully</response>
    /// <response code="404">Status history record not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEntityStatusHistory(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting entity status history {Id}", id);

            var deleted = await _entityStatusHistoryService.DeleteEntityStatusHistoryAsync(id);
            if (!deleted)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"Entity status history with ID '{id}' not found"
                };

                _logger.LogWarning("Entity status history {Id} not found for deletion", id);
                return NotFound(errorResponse);
            }

            _logger.LogInformation("Successfully deleted entity status history {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity status history {Id}", id);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while deleting the entity status history",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Gets the latest status history record for an entity
    /// </summary>
    /// <param name="entityId">The entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest status history record</returns>
    /// <response code="200">Latest status history retrieved successfully</response>
    /// <response code="404">Entity not found or no status history exists</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("entity/{entityId}/latest")]
    [ProducesResponseType(typeof(EntityStatusHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLatestEntityStatus(
        [FromRoute] string entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving latest entity status for entity {EntityId}", entityId);

            // Validate that the entity exists
            var entity = await _entityService.GetEntityAsync(entityId);
            if (entity == null)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"Entity with ID '{entityId}' not found"
                };

                _logger.LogWarning("Entity {EntityId} not found when retrieving latest status", entityId);
                return NotFound(errorResponse);
            }

            // Get the latest status history record
            var latestStatus = await _entityStatusHistoryService.GetLatestEntityStatusAsync(entityId);
            if (latestStatus == null)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"No status history found for entity '{entityId}'"
                };

                _logger.LogWarning("No status history found for entity {EntityId}", entityId);
                return NotFound(errorResponse);
            }

            var response = new EntityStatusHistoryResponse
            {
                Id = latestStatus.Id ?? 0,
                EntityId = latestStatus.EntityId,
                Status = latestStatus.Status,
                StatusMessage = latestStatus.StatusMessage,
                ResponseTime = latestStatus.ResponseTime,
                UptimePercentage = latestStatus.UptimePercentage,
                CheckedAt = latestStatus.CheckedAt,
                CreatedAt = latestStatus.CreatedOn ?? DateTime.UtcNow,
                UpdatedAt = latestStatus.ModifiedOn
            };

            _logger.LogInformation("Successfully retrieved latest entity status for entity {EntityId}", entityId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest entity status for entity {EntityId}", entityId);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while retrieving latest entity status",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    /// Gets summary statistics for an entity's status history
    /// </summary>
    /// <param name="entityId">The entity ID</param>
    /// <param name="fromDate">Start date for statistics (optional)</param>
    /// <param name="toDate">End date for statistics (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Summary statistics</returns>
    /// <response code="200">Summary statistics retrieved successfully</response>
    /// <response code="404">Entity not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("entity/{entityId}/summary")]
    [ProducesResponseType(typeof(EntityStatusSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEntityStatusSummary(
        [FromRoute] string entityId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving entity status summary for entity {EntityId}", entityId);

            // Validate that the entity exists
            var entity = await _entityService.GetEntityAsync(entityId);
            if (entity == null)
            {
                var errorResponse = new ApiErrorResponse
                {
                    Message = $"Entity with ID '{entityId}' not found"
                };

                _logger.LogWarning("Entity {EntityId} not found when retrieving status summary", entityId);
                return NotFound(errorResponse);
            }

            // Get summary data
            var statusCounts = await _entityStatusHistoryService.GetEntityStatusSummaryAsync(entityId, fromDate, toDate);
            var averageResponseTime = await _entityStatusHistoryService.GetAverageResponseTimeAsync(entityId, fromDate, toDate);
            var averageUptime = await _entityStatusHistoryService.GetAverageUptimeAsync(entityId, fromDate, toDate);
            var totalChecks = await _entityStatusHistoryService.GetEntityStatusHistoryCountAsync(entityId);

            var response = new EntityStatusSummaryResponse
            {
                EntityId = entityId,
                StatusCounts = statusCounts,
                AverageResponseTime = averageResponseTime,
                AverageUptime = averageUptime,
                TotalChecks = totalChecks,
                FromDate = fromDate,
                ToDate = toDate
            };

            _logger.LogInformation("Successfully retrieved entity status summary for entity {EntityId}", entityId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity status summary for entity {EntityId}", entityId);

            var errorResponse = new ApiErrorResponse
            {
                Message = "An error occurred while retrieving entity status summary",
                Details = ex.Message
            };

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
