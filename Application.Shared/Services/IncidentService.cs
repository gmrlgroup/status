using Application.Shared.Data;
using Application.Shared.Models;
using Application.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Shared.Services;

public class IncidentService : IIncidentService
{
    private readonly ApplicationDbContext _context;

    public IncidentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Incident>> GetIncidentsAsync(string workspaceId)
    {
        return await _context.Incident
            .Where(i => i.WorkspaceId == workspaceId)
            .Include(i => i.Entity)
            .Include(i => i.Updates.OrderByDescending(u => u.PostedAt))
            .OrderByDescending(i => i.StartedAt)
            .ToListAsync();
    }

    public async Task<List<Incident>> GetIncidentsByEntityAsync(string entityId)
    {
        return await _context.Incident
            .Where(i => i.EntityId == entityId)
            .Include(i => i.Entity)
            .Include(i => i.Updates.OrderByDescending(u => u.PostedAt))
            .OrderByDescending(i => i.StartedAt)
            .ToListAsync();
    }

    public async Task<Incident?> GetIncidentAsync(string id)
    {
        return await _context.Incident
            .Include(i => i.Entity)
            .Include(i => i.Updates.OrderByDescending(u => u.PostedAt))
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Incident>> GetActiveIncidentsAsync(string workspaceId)
    {
        return await _context.Incident
            .Where(i => i.WorkspaceId == workspaceId && i.Status != IncidentStatus.Resolved)
            .Include(i => i.Entity)
            .Include(i => i.Updates.OrderByDescending(u => u.PostedAt))
            .OrderByDescending(i => i.StartedAt)
            .ToListAsync();
    }

    public async Task<Incident> CreateIncidentAsync(Incident incident)
    {
        if (string.IsNullOrEmpty(incident.Id))
        {
            incident.Id = Guid.NewGuid().ToString();
        }

        incident.CreatedAt = DateTime.UtcNow;
        incident.UpdatedAt = DateTime.UtcNow;
        incident.StartedAt = DateTime.UtcNow;

        _context.Incident.Add(incident);
        await _context.SaveChangesAsync();

        return incident;
    }

    public async Task<Incident> UpdateIncidentAsync(Incident incident)
    {
        incident.UpdatedAt = DateTime.UtcNow;

        _context.Incident.Update(incident);
        await _context.SaveChangesAsync();

        return incident;
    }

    public async Task<Incident> UpdateIncidentStatusAsync(string incidentId, IncidentStatus status, string? message = null, string? updatedBy = null)
    {
        var incident = await GetIncidentAsync(incidentId);
        if (incident == null)
        {
            throw new ArgumentException("Incident not found", nameof(incidentId));
        }

        var previousStatus = incident.Status;
        incident.Status = status;
        incident.UpdatedAt = DateTime.UtcNow;

        // If resolving the incident, set the resolved timestamp
        if (status == IncidentStatus.Resolved && !incident.ResolvedAt.HasValue)
        {
            incident.ResolvedAt = DateTime.UtcNow;
        }

        // Create an incident update if message is provided or status changed
        if (!string.IsNullOrEmpty(message) || previousStatus != status)
        {
            var update = new IncidentUpdate
            {
                IncidentId = incidentId,
                Message = message ?? $"Status changed from {previousStatus} to {status}",
                StatusChange = status,
                Author = updatedBy,
                WorkspaceId = incident.WorkspaceId
            };

            await CreateIncidentUpdateAsync(update);
        }

        _context.Incident.Update(incident);
        await _context.SaveChangesAsync();

        return incident;
    }

    public async Task<Incident> ResolveIncidentAsync(string incidentId, string resolutionDetails, string? resolvedBy = null)
    {
        var incident = await GetIncidentAsync(incidentId);
        if (incident == null)
        {
            throw new ArgumentException("Incident not found", nameof(incidentId));
        }

        incident.Status = IncidentStatus.Resolved;
        incident.ResolvedAt = DateTime.UtcNow;
        incident.ResolutionDetails = resolutionDetails;
        incident.UpdatedAt = DateTime.UtcNow;

        // Create a resolution update
        var update = new IncidentUpdate
        {
            IncidentId = incidentId,
            Message = $"Incident resolved: {resolutionDetails}",
            StatusChange = IncidentStatus.Resolved,
            Author = resolvedBy,
            WorkspaceId = incident.WorkspaceId
        };

        await CreateIncidentUpdateAsync(update);

        _context.Incident.Update(incident);
        await _context.SaveChangesAsync();

        return incident;
    }

    public async Task<IncidentUpdate> CreateIncidentUpdateAsync(IncidentUpdate update)
    {
        if (string.IsNullOrEmpty(update.Id))
        {
            update.Id = Guid.NewGuid().ToString();
        }

        update.CreatedAt = DateTime.UtcNow;
        update.UpdatedAt = DateTime.UtcNow;
        update.PostedAt = DateTime.UtcNow;

        _context.IncidentUpdate.Add(update);
        await _context.SaveChangesAsync();

        return update;
    }

    public async Task<List<IncidentUpdate>> GetIncidentUpdatesAsync(string incidentId)
    {
        return await _context.IncidentUpdate
            .Where(u => u.IncidentId == incidentId)
            .OrderByDescending(u => u.PostedAt)
            .ToListAsync();
    }

    public async Task DeleteIncidentAsync(string id)
    {
        var incident = await _context.Incident.FindAsync(id);
        if (incident != null)
        {
            incident.IsDeleted = true;
            incident.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetActiveIncidentCountAsync(string workspaceId)
    {
        return await _context.Incident
            .CountAsync(i => i.WorkspaceId == workspaceId && i.Status != IncidentStatus.Resolved);
    }

    public async Task<int> GetCriticalIncidentCountAsync(string workspaceId)
    {
        return await _context.Incident
            .CountAsync(i => i.WorkspaceId == workspaceId && 
                           i.Status != IncidentStatus.Resolved && 
                           i.Severity == IncidentSeverity.Critical);
    }
}
