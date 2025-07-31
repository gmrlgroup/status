using Application.Shared.Data;
using Application.Shared.Models;
using Application.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Shared.Services;

public class EntityService : IEntityService
{
    private readonly ApplicationDbContext _context;

    public EntityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Entity>> GetEntitiesAsync(string workspaceId)
    {
        return await _context.Entity
            .Where(e => e.WorkspaceId == workspaceId)
            .Include(e => e.Dependencies)
            .Include(e => e.DependentOn)
            .Include(e => e.Jobs)
            .Include(e => e.StatusHistory.OrderByDescending(sh => sh.CheckedAt).Take(1))
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Entity?> GetEntityAsync(string id)
    {
        return await _context.Entity
            .Include(e => e.Dependencies)
            .Include(e => e.DependentOn)
            .Include(e => e.Jobs)
            .Include(e => e.StatusHistory.OrderByDescending(sh => sh.CheckedAt))
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Entity> CreateEntityAsync(Entity entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = Guid.NewGuid().ToString();
        }

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Entity.Add(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }

    public async Task<Entity> UpdateEntityAsync(Entity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        
        _context.Entity.Update(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }

    public async Task<bool> DeleteEntityAsync(string id)
    {
        var entity = await _context.Entity.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        _context.Entity.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Entity>> GetEntitiesByTypeAsync(string workspaceId, EntityType entityType)
    {
        return await _context.Entity
            .Where(e => e.WorkspaceId == workspaceId && e.EntityType == entityType)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<List<Entity>> GetCriticalEntitiesAsync(string workspaceId)
    {
        return await _context.Entity
            .Where(e => e.WorkspaceId == workspaceId && e.IsCritical)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<List<Entity>> GetActiveEntitiesAsync(string workspaceId)
    {
        return await _context.Entity
            .Where(e => e.WorkspaceId == workspaceId && e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<List<Entity>> GetEntitiesWithCurrentStatusAsync(string workspaceId, EntityStatus status)
    {
        return await _context.Entity
            .Where(e => e.WorkspaceId == workspaceId && 
                       e.StatusHistory.OrderByDescending(sh => sh.CheckedAt).FirstOrDefault()!.Status == status)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<EntityStatusHistory> AddEntityStatusAsync(string entityId, EntityStatus status, string? statusMessage = null, double? responseTime = null, double? uptimePercentage = null)
    {
        var statusHistory = new EntityStatusHistory
        {
            EntityId = entityId,
            Status = status,
            StatusMessage = statusMessage,
            ResponseTime = responseTime != null ? Math.Round(responseTime.Value, 2) : null,
            UptimePercentage = uptimePercentage != null ? Math.Round(uptimePercentage.Value, 2) : null,
            CheckedAt = DateTime.UtcNow
        };

        _context.EntityStatusHistory.Add(statusHistory);
        await _context.SaveChangesAsync();

        return statusHistory;
    }

    public async Task<EntityStatusHistory?> GetLatestEntityStatusAsync(string entityId)
    {
        return await _context.EntityStatusHistory
            .Where(sh => sh.EntityId == entityId)
            .OrderByDescending(sh => sh.CheckedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<EntityStatusHistory>> GetEntityStatusHistoryAsync(string entityId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.EntityStatusHistory
            .Where(sh => sh.EntityId == entityId);

        if (fromDate.HasValue)
        {
            query = query.Where(sh => sh.CheckedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(sh => sh.CheckedAt <= toDate.Value);
        }

        return await query
            .OrderByDescending(sh => sh.CheckedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateEntityUptimeAsync(string id, double uptimePercentage)
    {
        var entity = await _context.Entity.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        // Create a new status history entry with uptime update
        await AddEntityStatusAsync(id, EntityStatus.Unknown, "Uptime updated", null, uptimePercentage);
        return true;
    }

    public async Task<bool> UpdateEntityResponseTimeAsync(string id, double responseTime)
    {
        var entity = await _context.Entity.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        // Create a new status history entry with response time update
        await AddEntityStatusAsync(id, EntityStatus.Unknown, "Response time updated", responseTime);
        return true;
    }

    public async Task<bool> PingEntityAsync(string id)
    {
        var entity = await _context.Entity.FindAsync(id);
        if (entity == null || string.IsNullOrEmpty(entity.Url))
        {
            return false;
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await httpClient.GetAsync(entity.Url);
            stopwatch.Stop();

            var responseTime = stopwatch.Elapsed.TotalMilliseconds;
            var status = response.IsSuccessStatusCode ? EntityStatus.Online : EntityStatus.Error;
            var statusMessage = response.IsSuccessStatusCode 
                ? $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}" 
                : $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}";

            await AddEntityStatusAsync(id, status, statusMessage, responseTime);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            await AddEntityStatusAsync(id, EntityStatus.Offline, ex.Message);
            return false;
        }
    }

    public async Task<List<Entity>> SearchEntitiesAsync(string workspaceId, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetEntitiesAsync(workspaceId);
        }

        var lowerSearchTerm = searchTerm.ToLower();

        return await _context.Entity
            .Where(e => e.WorkspaceId == workspaceId && 
                       (e.Name.ToLower().Contains(lowerSearchTerm) ||
                        (e.Description != null && e.Description.ToLower().Contains(lowerSearchTerm)) ||
                        (e.Owner != null && e.Owner.ToLower().Contains(lowerSearchTerm)) ||
                        (e.Location != null && e.Location.ToLower().Contains(lowerSearchTerm))))
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Dictionary<EntityStatus, int>> GetEntityStatusSummaryAsync(string workspaceId)
    {
        var entities = await _context.Entity
            .Where(e => e.WorkspaceId == workspaceId)
            .Include(e => e.StatusHistory)
            .ToListAsync();

        return entities
            .Select(e => e.StatusHistory.OrderByDescending(sh => sh.CheckedAt).FirstOrDefault()?.Status ?? EntityStatus.Unknown)
            .GroupBy(status => status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<List<Entity>> GetEntitiesDueForCheckAsync()
    {
        var checkThreshold = DateTime.UtcNow.AddMinutes(-15); // Check every 15 minutes

        return await _context.Entity
            .Where(e => e.IsActive)
            .Include(e => e.StatusHistory)
            .ToListAsync()
            .ContinueWith(task => task.Result
                .Where(e => e.StatusHistory.OrderByDescending(sh => sh.CheckedAt).FirstOrDefault() == null || 
                           e.StatusHistory.OrderByDescending(sh => sh.CheckedAt).FirstOrDefault()!.CheckedAt < checkThreshold)
                .OrderBy(e => e.StatusHistory.OrderByDescending(sh => sh.CheckedAt).FirstOrDefault()?.CheckedAt ?? DateTime.MinValue)
                .ToList());
    }

    public async Task<bool> EntityExistsAsync(string id)
    {
        return await _context.Entity.AnyAsync(e => e.Id == id);
    }

    public async Task<List<Entity>> GetEntitiesWithLatestStatusAsync(string workspaceId)
    {
        return await _context.Entity
            .Where(e => e.WorkspaceId == workspaceId)
            .Include(e => e.StatusHistory.OrderByDescending(sh => sh.CheckedAt).Take(1))
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    // Dependency management methods
    public async Task<EntityDependency> CreateEntityDependencyAsync(EntityDependency dependency)
    {
        dependency.Id = Guid.NewGuid().ToString();
        dependency.CreatedAt = DateTime.UtcNow;
        dependency.UpdatedAt = DateTime.UtcNow;

        _context.EntityDependency.Add(dependency);
        await _context.SaveChangesAsync();
        
        // Reload with related entities
        return await _context.EntityDependency
            .Include(d => d.Entity)
            .Include(d => d.DependsOnEntity)
            .FirstOrDefaultAsync(d => d.Id == dependency.Id) ?? dependency;
    }

    public async Task<EntityDependency> UpdateEntityDependencyAsync(EntityDependency dependency)
    {
        var existingDependency = await _context.EntityDependency.FindAsync(dependency.Id);
        if (existingDependency == null)
        {
            throw new InvalidOperationException("Dependency not found");
        }

        existingDependency.DependsOnEntityId = dependency.DependsOnEntityId;
        existingDependency.Description = dependency.Description;
        existingDependency.IsActive = dependency.IsActive;
        existingDependency.IsCritical = dependency.IsCritical;
        existingDependency.DependencyType = dependency.DependencyType;
        existingDependency.Order = dependency.Order;
        existingDependency.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload with related entities
        return await _context.EntityDependency
            .Include(d => d.Entity)
            .Include(d => d.DependsOnEntity)
            .FirstOrDefaultAsync(d => d.Id == dependency.Id) ?? existingDependency;
    }

    public async Task<bool> DeleteEntityDependencyAsync(string dependencyId)
    {
        var dependency = await _context.EntityDependency.FindAsync(dependencyId);
        if (dependency == null)
        {
            return false;
        }

        _context.EntityDependency.Remove(dependency);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<EntityDependency>> GetEntityDependenciesAsync(string entityId)
    {
        return await _context.EntityDependency
            .Where(d => d.EntityId == entityId)
            .Include(d => d.DependsOnEntity)
            .OrderBy(d => d.Order)
            .ToListAsync();
    }

    public async Task<List<EntityDependency>> GetEntityDependentsAsync(string entityId)
    {
        return await _context.EntityDependency
            .Where(d => d.DependsOnEntityId == entityId)
            .Include(d => d.Entity)
            .OrderBy(d => d.Order)
            .ToListAsync();
    }

    public async Task<DependencyTree> GetEntityDependencyTreeAsync(string entityId)
    {
        var entity = await GetEntityAsync(entityId);
        if (entity == null)
        {
            throw new ArgumentException("Entity not found", nameof(entityId));
        }

        var latestStatus = await GetLatestEntityStatusAsync(entityId);

        var tree = new DependencyTree
        {
            EntityId = entity.Id,
            EntityName = entity.Name,
            EntityType = entity.EntityType,
            CurrentStatus = latestStatus?.Status ?? EntityStatus.Unknown
        };

        // Build dependency tree (what this entity depends on)
        tree.Dependencies = await BuildDependencyNodes(entityId, true, new HashSet<string>(), 0);
        
        // Build dependent tree (what depends on this entity)
        tree.Dependents = await BuildDependencyNodes(entityId, false, new HashSet<string>(), 0);

        return tree;
    }

    private async Task<List<DependencyTreeNode>> BuildDependencyNodes(string entityId, bool isDependency, HashSet<string> visitedEntities, int level)
    {
        // Prevent infinite loops in circular dependencies
        if (visitedEntities.Contains(entityId))
        {
            return new List<DependencyTreeNode>();
        }

        visitedEntities.Add(entityId);
        var nodes = new List<DependencyTreeNode>();

        List<EntityDependency> dependencies;
        if (isDependency)
        {
            // Get what this entity depends on
            dependencies = await _context.EntityDependency
                .Where(d => d.EntityId == entityId)
                .Include(d => d.DependsOnEntity)
                .OrderBy(d => d.Order)
                .ToListAsync();
        }
        else
        {
            // Get what depends on this entity
            dependencies = await _context.EntityDependency
                .Where(d => d.DependsOnEntityId == entityId)
                .Include(d => d.Entity)
                .OrderBy(d => d.Order)
                .ToListAsync();
        }

        foreach (var dependency in dependencies)
        {
            var targetEntity = isDependency ? dependency.DependsOnEntity : dependency.Entity;
            if (targetEntity == null) continue;

            var targetLatestStatus = await GetLatestEntityStatusAsync(targetEntity.Id);

            var node = new DependencyTreeNode
            {
                EntityId = targetEntity.Id,
                EntityName = targetEntity.Name,
                EntityType = targetEntity.EntityType,
                CurrentStatus = targetLatestStatus?.Status ?? EntityStatus.Unknown,
                IsCritical = dependency.IsCritical,
                IsActive = dependency.IsActive,
                Description = dependency.Description,
                Order = dependency.Order,
                Level = level
            };

            // Recursively build children (limit depth to prevent excessive nesting)
            if (level < 5) // Max depth of 5 levels
            {
                var newVisited = new HashSet<string>(visitedEntities);
                node.Children = await BuildDependencyNodes(targetEntity.Id, isDependency, newVisited, level + 1);
            }

            nodes.Add(node);
        }

        visitedEntities.Remove(entityId);
        return nodes;
    }
}
