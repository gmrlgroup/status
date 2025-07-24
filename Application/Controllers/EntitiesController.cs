using Application.Shared.Models;
using Application.Shared.Services;
using Application.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Application.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class EntitiesController : ControllerBase
{
    private readonly IEntityService _entityService;

    public EntitiesController(IEntityService entityService)
    {
        _entityService = entityService;
    }

    // GET: api/Entities
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Entity>>> GetEntities()
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var entities = await _entityService.GetEntitiesAsync(workspaceId);
        return Ok(entities);
    }

    // GET: api/Entities/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Entity>> GetEntity(string id)
    {
        var entity = await _entityService.GetEntityAsync(id);
        
        if (entity == null)
        {
            return NotFound();
        }

        return Ok(entity);
    }

    // POST: api/Entities
    [HttpPost]
    public async Task<ActionResult<Entity>> CreateEntity(Entity entity)
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        entity.WorkspaceId = workspaceId;
        var createdEntity = await _entityService.CreateEntityAsync(entity);
        
        return CreatedAtAction(nameof(GetEntity), new { id = createdEntity.Id }, createdEntity);
    }

    // PUT: api/Entities/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEntity(string id, Entity entity)
    {
        if (id != entity.Id)
        {
            return BadRequest("Entity ID mismatch");
        }

        var existingEntity = await _entityService.GetEntityAsync(id);
        if (existingEntity == null)
        {
            return NotFound();
        }

        await _entityService.UpdateEntityAsync(entity);
        return NoContent();
    }

    // DELETE: api/Entities/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntity(string id)
    {
        var success = await _entityService.DeleteEntityAsync(id);
        
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // GET: api/Entities/type/{entityType}
    [HttpGet("type/{entityType}")]
    public async Task<ActionResult<IEnumerable<Entity>>> GetEntitiesByType(EntityType entityType)
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var entities = await _entityService.GetEntitiesByTypeAsync(workspaceId, entityType);
        return Ok(entities);
    }

    // GET: api/Entities/critical
    [HttpGet("critical")]
    public async Task<ActionResult<IEnumerable<Entity>>> GetCriticalEntities()
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var entities = await _entityService.GetCriticalEntitiesAsync(workspaceId);
        return Ok(entities);
    }

    // GET: api/Entities/active
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Entity>>> GetActiveEntities()
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var entities = await _entityService.GetActiveEntitiesAsync(workspaceId);
        return Ok(entities);
    }

    // GET: api/Entities/status/{status}
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<Entity>>> GetEntitiesByStatus(EntityStatus status)
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var entities = await _entityService.GetEntitiesWithCurrentStatusAsync(workspaceId, status);
        return Ok(entities);
    }

    // POST: api/Entities/{id}/status
    [HttpPost("{id}/status")]
    public async Task<IActionResult> UpdateEntityStatus(string id, [FromBody] UpdateStatusRequest request)
    {
        var statusHistory = await _entityService.AddEntityStatusAsync(id, request.Status, request.StatusMessage);
        
        if (statusHistory == null)
        {
            return NotFound();
        }

        return Ok(statusHistory);
    }

    // POST: api/Entities/{id}/ping
    [HttpPost("{id}/ping")]
    public async Task<ActionResult<bool>> PingEntity(string id)
    {
        var success = await _entityService.PingEntityAsync(id);
        return Ok(success);
    }

    // GET: api/Entities/search
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Entity>>> SearchEntities([FromQuery] string searchTerm)
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var entities = await _entityService.SearchEntitiesAsync(workspaceId, searchTerm);
        return Ok(entities);
    }

    // GET: api/Entities/summary
    [HttpGet("summary")]
    public async Task<ActionResult<Dictionary<EntityStatus, int>>> GetEntityStatusSummary()
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var summary = await _entityService.GetEntityStatusSummaryAsync(workspaceId);
        return Ok(summary);
    }

    // GET: api/Entities/health-check
    [HttpGet("health-check")]
    public async Task<ActionResult<IEnumerable<Entity>>> GetEntitiesDueForCheck()
    {
        var entities = await _entityService.GetEntitiesDueForCheckAsync();
        return Ok(entities);
    }

    // GET: api/Entities/{id}/dependencies
    [HttpGet("{id}/dependencies")]
    public async Task<ActionResult<IEnumerable<EntityDependency>>> GetEntityDependencies(string id)
    {
        var dependencies = await _entityService.GetEntityDependenciesAsync(id);
        return Ok(dependencies);
    }

    // POST: api/Entities/dependencies
    [HttpPost("dependencies")]
    public async Task<ActionResult<EntityDependency>> CreateEntityDependency(EntityDependency dependency)
    {
        var createdDependency = await _entityService.CreateEntityDependencyAsync(dependency);
        return CreatedAtAction(nameof(GetEntityDependencies), new { id = dependency.EntityId }, createdDependency);
    }

    // PUT: api/Entities/dependencies/{id}
    [HttpPut("dependencies/{id}")]
    public async Task<IActionResult> UpdateEntityDependency(string id, EntityDependency dependency)
    {
        if (id != dependency.Id)
        {
            return BadRequest("Dependency ID mismatch");
        }

        await _entityService.UpdateEntityDependencyAsync(dependency);
        return NoContent();
    }

    // DELETE: api/Entities/dependencies/{id}
    [HttpDelete("dependencies/{id}")]
    public async Task<IActionResult> DeleteEntityDependency(string id)
    {
        var success = await _entityService.DeleteEntityDependencyAsync(id);
        
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}

public class UpdateStatusRequest
{
    public EntityStatus Status { get; set; }
    public string? StatusMessage { get; set; }
}
