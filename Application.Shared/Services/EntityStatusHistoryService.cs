using Application.Shared.Data;
using Application.Shared.Models;
using Application.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Shared.Services;

public class EntityStatusHistoryService : IEntityStatusHistoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EntityStatusHistoryService> _logger;

    public EntityStatusHistoryService(ApplicationDbContext context, ILogger<EntityStatusHistoryService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<EntityStatusHistory>> GetEntityStatusHistoryAsync(string entityId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.EntityStatusHistory
                .Where(h => h.EntityId == entityId && !h.IsDeleted);

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.CheckedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(h => h.CheckedAt <= toDate.Value);
            }

            return await query
                .OrderByDescending(h => h.CheckedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity status history for entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<EntityStatusHistory?> GetEntityStatusHistoryByIdAsync(int id)
    {
        try
        {
            return await _context.EntityStatusHistory
                .Include(h => h.Entity)
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity status history by ID {Id}", id);
            throw;
        }
    }

    public async Task<EntityStatusHistory?> GetLatestEntityStatusAsync(string entityId)
    {
        try
        {
            return await _context.EntityStatusHistory
                .Where(h => h.EntityId == entityId && !h.IsDeleted)
                .OrderByDescending(h => h.CheckedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest entity status for entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<List<EntityStatusHistory>> GetEntityStatusHistoryByStatusAsync(string entityId, EntityStatus status)
    {
        try
        {
            return await _context.EntityStatusHistory
                .Where(h => h.EntityId == entityId && h.Status == status && !h.IsDeleted)
                .OrderByDescending(h => h.CheckedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity status history by status {Status} for entity {EntityId}", status, entityId);
            throw;
        }
    }

    public async Task<EntityStatusHistory> CreateEntityStatusHistoryAsync(EntityStatusHistory statusHistory)
    {
        try
        {
            if (statusHistory == null)
                throw new ArgumentNullException(nameof(statusHistory));

            // Validate that the entity exists
            var entityExists = await _context.Entity.AnyAsync(e => e.Id == statusHistory.EntityId && !e.IsDeleted);
            if (!entityExists)
            {
                throw new ArgumentException($"Entity with ID '{statusHistory.EntityId}' not found", nameof(statusHistory));
            }

            statusHistory.CreatedOn = DateTime.UtcNow;
            statusHistory.ModifiedOn = DateTime.UtcNow;
            statusHistory.CheckedAt = statusHistory.CheckedAt == default ? DateTime.UtcNow : statusHistory.CheckedAt;

            _context.EntityStatusHistory.Add(statusHistory);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created entity status history record {Id} for entity {EntityId}", 
                statusHistory.Id, statusHistory.EntityId);

            return statusHistory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity status history for entity {EntityId}", statusHistory?.EntityId);
            throw;
        }
    }

    public async Task<EntityStatusHistory> UpdateEntityStatusHistoryAsync(EntityStatusHistory statusHistory)
    {
        try
        {
            if (statusHistory == null)
                throw new ArgumentNullException(nameof(statusHistory));

            var existingRecord = await _context.EntityStatusHistory
                .FirstOrDefaultAsync(h => h.Id == statusHistory.Id && !h.IsDeleted);

            if (existingRecord == null)
            {
                throw new ArgumentException($"Entity status history with ID '{statusHistory.Id}' not found", nameof(statusHistory));
            }

            // Update properties
            existingRecord.Status = statusHistory.Status;
            existingRecord.StatusMessage = statusHistory.StatusMessage;
            existingRecord.ResponseTime = statusHistory.ResponseTime;
            existingRecord.UptimePercentage = statusHistory.UptimePercentage;
            existingRecord.CheckedAt = statusHistory.CheckedAt;
            existingRecord.ModifiedOn = DateTime.UtcNow;
            existingRecord.ModifiedBy = statusHistory.ModifiedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated entity status history record {Id} for entity {EntityId}", 
                statusHistory.Id, statusHistory.EntityId);

            return existingRecord;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity status history {Id}", statusHistory?.Id);
            throw;
        }
    }

    public async Task<bool> DeleteEntityStatusHistoryAsync(int id)
    {
        try
        {
            var statusHistory = await _context.EntityStatusHistory
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

            if (statusHistory == null)
            {
                _logger.LogWarning("Entity status history {Id} not found for deletion", id);
                return false;
            }

            // Soft delete
            statusHistory.IsDeleted = true;
            statusHistory.DeletedAt = DateTime.UtcNow;
            statusHistory.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted entity status history record {Id}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity status history {Id}", id);
            throw;
        }
    }

    public async Task<List<EntityStatusHistory>> GetEntityStatusHistoryByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            return await _context.EntityStatusHistory
                .Include(h => h.Entity)
                .Where(h => h.CheckedAt >= fromDate && h.CheckedAt <= toDate && !h.IsDeleted)
                .OrderByDescending(h => h.CheckedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity status history by date range {FromDate} to {ToDate}", fromDate, toDate);
            throw;
        }
    }

    public async Task<List<EntityStatusHistory>> GetAllEntityStatusHistoryAsync(string workspaceId)
    {
        try
        {
            return await _context.EntityStatusHistory
                .Include(h => h.Entity)
                .Where(h => h.WorkspaceId == workspaceId && !h.IsDeleted)
                .OrderByDescending(h => h.CheckedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entity status history for workspace {WorkspaceId}", workspaceId);
            throw;
        }
    }

    public async Task<Dictionary<EntityStatus, int>> GetEntityStatusSummaryAsync(string entityId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.EntityStatusHistory
                .Where(h => h.EntityId == entityId && !h.IsDeleted);

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.CheckedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(h => h.CheckedAt <= toDate.Value);
            }

            return await query
                .GroupBy(h => h.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity status summary for entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<double> GetAverageResponseTimeAsync(string entityId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.EntityStatusHistory
                .Where(h => h.EntityId == entityId && h.ResponseTime.HasValue && !h.IsDeleted);

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.CheckedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(h => h.CheckedAt <= toDate.Value);
            }

            return await query.AverageAsync(h => h.ResponseTime!.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average response time for entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<double> GetAverageUptimeAsync(string entityId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.EntityStatusHistory
                .Where(h => h.EntityId == entityId && h.UptimePercentage.HasValue && !h.IsDeleted);

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.CheckedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(h => h.CheckedAt <= toDate.Value);
            }

            return await query.AverageAsync(h => h.UptimePercentage!.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average uptime for entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<List<EntityStatusHistory>> GetEntityStatusHistoryWithPaginationAsync(string entityId, int page, int pageSize)
    {
        try
        {
            return await _context.EntityStatusHistory
                .Where(h => h.EntityId == entityId && !h.IsDeleted)
                .OrderByDescending(h => h.CheckedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated entity status history for entity {EntityId}, page {Page}, pageSize {PageSize}", 
                entityId, page, pageSize);
            throw;
        }
    }

    public async Task<int> GetEntityStatusHistoryCountAsync(string entityId)
    {
        try
        {
            return await _context.EntityStatusHistory
                .CountAsync(h => h.EntityId == entityId && !h.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity status history count for entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<bool> EntityStatusHistoryExistsAsync(int id)
    {
        try
        {
            return await _context.EntityStatusHistory
                .AnyAsync(h => h.Id == id && !h.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if entity status history {Id} exists", id);
            throw;
        }
    }
}
