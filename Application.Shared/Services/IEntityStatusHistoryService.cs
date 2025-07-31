using Application.Shared.Models;
using Application.Shared.Enums;

namespace Application.Shared.Services;

public interface IEntityStatusHistoryService
{
    Task<List<EntityStatusHistory>> GetEntityStatusHistoryAsync(string entityId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<EntityStatusHistory?> GetEntityStatusHistoryByIdAsync(int id);
    Task<EntityStatusHistory?> GetLatestEntityStatusAsync(string entityId);
    Task<List<EntityStatusHistory>> GetEntityStatusHistoryByStatusAsync(string entityId, EntityStatus status);
    Task<EntityStatusHistory> CreateEntityStatusHistoryAsync(EntityStatusHistory statusHistory);
    Task<EntityStatusHistory> UpdateEntityStatusHistoryAsync(EntityStatusHistory statusHistory);
    Task<bool> DeleteEntityStatusHistoryAsync(int id);
    Task<List<EntityStatusHistory>> GetEntityStatusHistoryByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<List<EntityStatusHistory>> GetAllEntityStatusHistoryAsync(string workspaceId);
    Task<Dictionary<EntityStatus, int>> GetEntityStatusSummaryAsync(string entityId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<double> GetAverageResponseTimeAsync(string entityId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<double> GetAverageUptimeAsync(string entityId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<EntityStatusHistory>> GetEntityStatusHistoryWithPaginationAsync(string entityId, int page, int pageSize);
    Task<int> GetEntityStatusHistoryCountAsync(string entityId);
    Task<bool> EntityStatusHistoryExistsAsync(int id);
}
