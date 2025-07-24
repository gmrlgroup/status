using Application.Shared.Models;
using Application.Shared.Services;
using Application.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Application.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidentService;

    public IncidentsController(IIncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    // GET: api/Incidents
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Incident>>> GetIncidents()
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var incidents = await _incidentService.GetIncidentsAsync(workspaceId);
        return Ok(incidents);
    }

    // GET: api/Incidents/entity/{entityId}
    [HttpGet("entity/{entityId}")]
    public async Task<ActionResult<IEnumerable<Incident>>> GetIncidentsByEntity(string entityId)
    {
        var incidents = await _incidentService.GetIncidentsByEntityAsync(entityId);
        return Ok(incidents);
    }

    // GET: api/Incidents/active
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Incident>>> GetActiveIncidents()
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var incidents = await _incidentService.GetActiveIncidentsAsync(workspaceId);
        return Ok(incidents);
    }

    // GET: api/Incidents/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Incident>> GetIncident(string id)
    {
        var incident = await _incidentService.GetIncidentAsync(id);
        
        if (incident == null)
        {
            return NotFound();
        }

        return Ok(incident);
    }

    // POST: api/Incidents
    [HttpPost]
    public async Task<ActionResult<Incident>> CreateIncident(Incident incident)
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        incident.WorkspaceId = workspaceId;
        incident.CreatedBy = User?.Identity?.Name ?? "System";

        try
        {
            var createdIncident = await _incidentService.CreateIncidentAsync(incident);
            return CreatedAtAction(nameof(GetIncident), new { id = createdIncident.Id }, createdIncident);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating incident: {ex.Message}");
        }
    }

    // PUT: api/Incidents/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIncident(string id, Incident incident)
    {
        if (id != incident.Id)
        {
            return BadRequest("ID mismatch");
        }

        incident.ModifiedBy = User?.Identity?.Name ?? "System";

        try
        {
            await _incidentService.UpdateIncidentAsync(incident);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"Error updating incident: {ex.Message}");
        }
    }

    // PUT: api/Incidents/{id}/status
    [HttpPut("{id}/status")]
    public async Task<ActionResult<Incident>> UpdateIncidentStatus(string id, [FromBody] UpdateIncidentStatusRequest request)
    {
        try
        {
            var updatedBy = User?.Identity?.Name ?? "System";
            var incident = await _incidentService.UpdateIncidentStatusAsync(id, request.Status, request.Message, updatedBy);
            return Ok(incident);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error updating incident status: {ex.Message}");
        }
    }

    // PUT: api/Incidents/{id}/resolve
    [HttpPut("{id}/resolve")]
    public async Task<ActionResult<Incident>> ResolveIncident(string id, [FromBody] ResolveIncidentRequest request)
    {
        try
        {
            var resolvedBy = User?.Identity?.Name ?? "System";
            var incident = await _incidentService.ResolveIncidentAsync(id, request.ResolutionDetails, resolvedBy);
            return Ok(incident);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error resolving incident: {ex.Message}");
        }
    }

    // POST: api/Incidents/{id}/updates
    [HttpPost("{id}/updates")]
    public async Task<ActionResult<IncidentUpdate>> CreateIncidentUpdate(string id, [FromBody] CreateIncidentUpdateRequest request)
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var update = new IncidentUpdate
        {
            IncidentId = id,
            Message = request.Message,
            StatusChange = request.StatusChange,
            Author = User?.Identity?.Name ?? "System",
            WorkspaceId = workspaceId
        };

        try
        {
            var createdUpdate = await _incidentService.CreateIncidentUpdateAsync(update);
            return Ok(createdUpdate);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating incident update: {ex.Message}");
        }
    }

    // GET: api/Incidents/{id}/updates
    [HttpGet("{id}/updates")]
    public async Task<ActionResult<IEnumerable<IncidentUpdate>>> GetIncidentUpdates(string id)
    {
        var updates = await _incidentService.GetIncidentUpdatesAsync(id);
        return Ok(updates);
    }

    // DELETE: api/Incidents/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIncident(string id)
    {
        try
        {
            await _incidentService.DeleteIncidentAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"Error deleting incident: {ex.Message}");
        }
    }

    // GET: api/Incidents/stats/active-count
    [HttpGet("stats/active-count")]
    public async Task<ActionResult<int>> GetActiveIncidentCount()
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var count = await _incidentService.GetActiveIncidentCountAsync(workspaceId);
        return Ok(count);
    }

    // GET: api/Incidents/stats/critical-count
    [HttpGet("stats/critical-count")]
    public async Task<ActionResult<int>> GetCriticalIncidentCount()
    {
        var workspaceId = Request.Headers["X-Workspace-ID"].ToString();
        
        if (string.IsNullOrEmpty(workspaceId))
        {
            return BadRequest("Workspace ID is required in headers");
        }

        var count = await _incidentService.GetCriticalIncidentCountAsync(workspaceId);
        return Ok(count);
    }
}

// Request DTOs
public class UpdateIncidentStatusRequest
{
    public IncidentStatus Status { get; set; }
    public string? Message { get; set; }
}

public class ResolveIncidentRequest
{
    public string ResolutionDetails { get; set; } = string.Empty;
}

public class CreateIncidentUpdateRequest
{
    public string Message { get; set; } = string.Empty;
    public IncidentStatus? StatusChange { get; set; }
}
