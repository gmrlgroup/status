using Application.Shared.Models;
using Application.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Client.Services;

public class EntityStatusHistoryClientService
{
    private readonly HttpClient _httpClient;

    public EntityStatusHistoryClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<EntityStatusHistory>> GetEntityStatusHistoryAsync(string entityId, string workspaceId, int days = 7)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Workspace-ID");
            _httpClient.DefaultRequestHeaders.Add("X-Workspace-ID", workspaceId);
            
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var toDate = DateTime.UtcNow;
            
            var response = await _httpClient.GetAsync($"api/entity-status-history/entity/{entityId}?fromDate={fromDate:yyyy-MM-ddTHH:mm:ss.fffZ}&toDate={toDate:yyyy-MM-ddTHH:mm:ss.fffZ}&pageSize=100");
            
            if (response.IsSuccessStatusCode)
            {
                var pagedResponse = await response.Content.ReadFromJsonAsync<EntityStatusHistoryPagedResponse>();
                
                if (pagedResponse?.Items != null)
                {
                    // Convert from API response models to domain models
                    return pagedResponse.Items.Select(item => new EntityStatusHistory
                    {
                        Id = item.Id,
                        EntityId = item.EntityId,
                        Status = item.Status,
                        StatusMessage = item.StatusMessage,
                        ResponseTime = item.ResponseTime,
                        UptimePercentage = item.UptimePercentage,
                        CheckedAt = item.CheckedAt,
                        CreatedOn = item.CreatedAt,
                        ModifiedOn = item.UpdatedAt
                    }).ToList();
                }
            }
            
            return new List<EntityStatusHistory>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching entity status history: {ex.Message}");
            return new List<EntityStatusHistory>();
        }
    }

    public async Task<EntityStatusHistory?> GetLatestEntityStatusAsync(string entityId, string workspaceId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Workspace-ID");
            _httpClient.DefaultRequestHeaders.Add("X-Workspace-ID", workspaceId);
            
            var response = await _httpClient.GetAsync($"api/entity-status-history/entity/{entityId}/latest");
            
            if (response.IsSuccessStatusCode)
            {
                var latestResponse = await response.Content.ReadFromJsonAsync<EntityStatusHistoryResponse>();
                
                if (latestResponse != null)
                {
                    return new EntityStatusHistory
                    {
                        Id = latestResponse.Id,
                        EntityId = latestResponse.EntityId,
                        Status = latestResponse.Status,
                        StatusMessage = latestResponse.StatusMessage,
                        ResponseTime = latestResponse.ResponseTime,
                        UptimePercentage = latestResponse.UptimePercentage,
                        CheckedAt = latestResponse.CheckedAt,
                        CreatedOn = latestResponse.CreatedAt,
                        ModifiedOn = latestResponse.UpdatedAt
                    };
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching latest entity status: {ex.Message}");
            return null;
        }
    }

    public async Task<Dictionary<string, object>> GetEntityStatusSummaryAsync(string entityId, string workspaceId, int days = 30)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Workspace-ID");
            _httpClient.DefaultRequestHeaders.Add("X-Workspace-ID", workspaceId);
            
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var toDate = DateTime.UtcNow;
            
            var response = await _httpClient.GetAsync($"api/entity-status-history/entity/{entityId}/summary?fromDate={fromDate:yyyy-MM-ddTHH:mm:ss.fffZ}&toDate={toDate:yyyy-MM-ddTHH:mm:ss.fffZ}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var summary = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse, options);
                return summary ?? new Dictionary<string, object>();
            }
            
            return new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching entity status summary: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }
}
