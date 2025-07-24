using Application.Shared.Models;
using Application.Shared.Enums;

namespace Application.Shared.Services;

public interface IIncidentService
{
    Task<List<Incident>> GetIncidentsAsync(string workspaceId);
    Task<List<Incident>> GetIncidentsByEntityAsync(string entityId);
    Task<Incident?> GetIncidentAsync(string id);
    Task<List<Incident>> GetActiveIncidentsAsync(string workspaceId);
    Task<Incident> CreateIncidentAsync(Incident incident);
    Task<Incident> UpdateIncidentAsync(Incident incident);
    Task<Incident> UpdateIncidentStatusAsync(string incidentId, IncidentStatus status, string? message = null, string? updatedBy = null);
    Task<Incident> ResolveIncidentAsync(string incidentId, string resolutionDetails, string? resolvedBy = null);
    Task<IncidentUpdate> CreateIncidentUpdateAsync(IncidentUpdate update);
    Task<List<IncidentUpdate>> GetIncidentUpdatesAsync(string incidentId);
    Task DeleteIncidentAsync(string id);
    Task<int> GetActiveIncidentCountAsync(string workspaceId);
    Task<int> GetCriticalIncidentCountAsync(string workspaceId);
}
