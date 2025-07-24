using Application.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Client.Services;

public class IncidentClientService
{
    private readonly HttpClient _httpClient;

    public IncidentClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Incident>> GetIncidentsAsync(string workspaceId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Workspace-ID");
            _httpClient.DefaultRequestHeaders.Add("X-Workspace-ID", workspaceId);
            
            var response = await _httpClient.GetAsync("api/incidents");
            
            if (response.IsSuccessStatusCode)
            {
                var incidents = await response.Content.ReadFromJsonAsync<List<Incident>>();
                return incidents ?? new List<Incident>();
            }
            
            return new List<Incident>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching incidents: {ex.Message}");
            return new List<Incident>();
        }
    }

    public async Task<List<Incident>> GetActiveIncidentsAsync(string workspaceId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Workspace-ID");
            _httpClient.DefaultRequestHeaders.Add("X-Workspace-ID", workspaceId);
            
            var response = await _httpClient.GetAsync("api/incidents/active");
            
            if (response.IsSuccessStatusCode)
            {
                var incidents = await response.Content.ReadFromJsonAsync<List<Incident>>();
                return incidents ?? new List<Incident>();
            }
            
            return new List<Incident>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching active incidents: {ex.Message}");
            return new List<Incident>();
        }
    }

    public async Task<List<Incident>> GetIncidentsByEntityAsync(string entityId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/incidents/entity/{entityId}");
            
            if (response.IsSuccessStatusCode)
            {
                var incidents = await response.Content.ReadFromJsonAsync<List<Incident>>();
                return incidents ?? new List<Incident>();
            }
            
            return new List<Incident>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching incidents for entity: {ex.Message}");
            return new List<Incident>();
        }
    }

    public async Task<List<IncidentUpdate>> GetIncidentUpdatesAsync(string incidentId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/incidents/{incidentId}/updates");
            
            if (response.IsSuccessStatusCode)
            {
                var updates = await response.Content.ReadFromJsonAsync<List<IncidentUpdate>>();
                return updates ?? new List<IncidentUpdate>();
            }
            
            return new List<IncidentUpdate>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching incident updates: {ex.Message}");
            return new List<IncidentUpdate>();
        }
    }
}
