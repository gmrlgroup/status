using Application.Shared.Models;
using Application.Shared.Enums;

namespace Application.Shared.Services;

public interface IEntityService
{
    Task<List<Entity>> GetEntitiesAsync(string workspaceId);
    Task<Entity?> GetEntityAsync(string id);
    Task<Entity> CreateEntityAsync(Entity entity);
    Task<Entity> UpdateEntityAsync(Entity entity);
    Task<bool> DeleteEntityAsync(string id);
    Task<List<Entity>> GetEntitiesByTypeAsync(string workspaceId, EntityType entityType);
    Task<List<Entity>> GetCriticalEntitiesAsync(string workspaceId);
    Task<List<Entity>> GetActiveEntitiesAsync(string workspaceId);
    Task<List<Entity>> GetEntitiesWithCurrentStatusAsync(string workspaceId, EntityStatus status);
    Task<EntityStatusHistory> AddEntityStatusAsync(string entityId, EntityStatus status, string? statusMessage = null, double? responseTime = null, double? uptimePercentage = null);
    Task<EntityStatusHistory?> GetLatestEntityStatusAsync(string entityId);
    Task<List<EntityStatusHistory>> GetEntityStatusHistoryAsync(string entityId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> UpdateEntityUptimeAsync(string id, double uptimePercentage);
    Task<bool> UpdateEntityResponseTimeAsync(string id, double responseTime);
    Task<bool> PingEntityAsync(string id);
    Task<List<Entity>> SearchEntitiesAsync(string workspaceId, string searchTerm);
    Task<Dictionary<EntityStatus, int>> GetEntityStatusSummaryAsync(string workspaceId);
    Task<List<Entity>> GetEntitiesDueForCheckAsync();
    Task<bool> EntityExistsAsync(string id);
    Task<List<Entity>> GetEntitiesWithLatestStatusAsync(string workspaceId);
    
    // Dependency management methods
    Task<EntityDependency> CreateEntityDependencyAsync(EntityDependency dependency);
    Task<EntityDependency> UpdateEntityDependencyAsync(EntityDependency dependency);
    Task<bool> DeleteEntityDependencyAsync(string dependencyId);
    Task<List<EntityDependency>> GetEntityDependenciesAsync(string entityId);
    Task<List<EntityDependency>> GetEntityDependentsAsync(string entityId);
    Task<DependencyTree> GetEntityDependencyTreeAsync(string entityId);
}
