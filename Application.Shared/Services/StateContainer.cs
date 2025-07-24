using Application.Shared.Models;

namespace Application.Shared.Services;

public class StateContainer
{
    private string? savedString;
    private Workspace? workspace;
    private string? currentBranchId;
    private string? currentUserId;    public Workspace? Workspace
    {
        get => workspace;
        set
        {
            workspace = value;
            NotifyStateChanged();
        }
    }

    public string? BranchId
    {
        get => currentBranchId;
        set
        {
            currentBranchId = value;
            NotifyStateChanged();
        }
    }

    public string? UserId
    {
        get => currentUserId;
        set
        {
            currentUserId = value;
            NotifyStateChanged();
        }
    }

    public string Property
    {
        get => savedString ?? string.Empty;
        set
        {
            savedString = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}
