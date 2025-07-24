using Application.Shared.Models;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Application.Models;


public enum ResponseStatus
{
    Success,
    PartialSuccess,
    Error
}

public class Response<T>
{
    public T Items { get; set; }
    public Dictionary<string, string> Filters { get; set; }
    
    public DataState DataState { get; set; }

    public int TotalItems { get; set; }

    public ResponseStatus Status { get; set; }
    public string Message { get; set; }
    public void SetTotalItems(int totalItems)
    {
        TotalItems = totalItems;
    }

}

public class DataState
{
    public int Page { get; set; } = 0;

    public int PageSize { get; set; }
    public string? SortLabel { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

}
