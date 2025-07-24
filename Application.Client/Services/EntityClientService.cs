using Application.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Client.Services;

public class EntityClientService
{
    private readonly HttpClient _httpClient;

    public EntityClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Entity>> GetEntitiesAsync(string workspaceId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Workspace-ID");
            _httpClient.DefaultRequestHeaders.Add("X-Workspace-ID", workspaceId);
            
            var response = await _httpClient.GetAsync("api/entities");
            
            if (response.IsSuccessStatusCode)
            {
                var entities = await response.Content.ReadFromJsonAsync<List<Entity>>();
                return entities ?? new List<Entity>();
            }
            
            return new List<Entity>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching entities: {ex.Message}");
            return new List<Entity>();
        }
    }

    public async Task<Entity?> GetEntityAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/entities/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Entity>();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching entity: {ex.Message}");
            return null;
        }
    }
}
