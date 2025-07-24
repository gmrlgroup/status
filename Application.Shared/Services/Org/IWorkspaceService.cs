using Application.Shared.Models;

namespace Application.Shared.Services.Org
{
    public interface IWorkspaceService
    {
        Task<List<Workspace>> GetWorkspaces(string userId);

        Task<Workspace> GetWorkspace(string id);

        Task<Workspace> GetWorkspace(string id, string userId);
        Task<bool> UserIsWorkspaceMember(string id, string userId);

        Task<WorkspaceMember> AddWorkspaceMember(string workspaceId, string userId);

        Task<WorkspaceMember> AddWorkspaceMember(string workspaceId, string userId, MemberRole role);

        Task<WorkspaceMember> AddWorkspaceMemberByDomain(string domain, string userId);

        Task<Workspace> CreateWorkspace(Workspace workspace, string userId);
    }
}
