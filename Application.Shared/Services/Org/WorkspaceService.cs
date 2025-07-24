using Application.Shared.Data;
using Application.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Application.Shared.Services.Org
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly ApplicationDbContext _context;

        public WorkspaceService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<List<Workspace>> GetWorkspaces(string userId)
        {
            var workspaceMembersList = await _context.WorkspaceMember.Where(m => m.ApplicationUserId == userId).ToListAsync();
            var workspaceMembers = workspaceMembersList.Select(m => m.WorkspaceId).ToArray();

            return await _context.Workspace.Where(c => workspaceMembers.Contains(c.Id)).ToListAsync();
        }
        public async Task<Workspace> GetWorkspace(string id)
        {
            return (await _context.Workspace.FindAsync(id))!;
        }

        public async Task<Workspace> GetWorkspace(string id, string userId)
        {

            var workspaceMembersList = await _context.WorkspaceMember.Where(m => m.ApplicationUserId == userId).ToListAsync();
            var workspaceMembers = workspaceMembersList.Select(m => m.WorkspaceId).ToArray();

            return (await _context.Workspace.FirstOrDefaultAsync(c => c.Id == id && workspaceMembers.Contains(c.Id)))!;

        }


        public async Task<bool> UserIsWorkspaceMember(string workspaceId, string userId)
        {
            return await _context.WorkspaceMember.AnyAsync(m => m.WorkspaceId == workspaceId && m.ApplicationUserId == userId);
        }


        public async Task<WorkspaceMember> AddWorkspaceMember(string workspaceId, string userId)
        {
            var workspaceMember = new WorkspaceMember
            {
                WorkspaceId = workspaceId,
                ApplicationUserId = userId
            };

            _context.WorkspaceMember.Add(workspaceMember);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WorkspaceMemberExists(workspaceMember.WorkspaceId, workspaceMember.ApplicationUserId))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return workspaceMember;
        }

        public async Task<WorkspaceMember> AddWorkspaceMember(string workspaceId, string userId, MemberRole role)
        {
            var workspaceMember = new WorkspaceMember
            {
                WorkspaceId = workspaceId,
                ApplicationUserId = userId,
                Role = role
            };

            _context.WorkspaceMember.Add(workspaceMember);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WorkspaceMemberExists(workspaceMember.WorkspaceId, workspaceMember.ApplicationUserId))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return workspaceMember;
        }


        public async Task<WorkspaceMember> AddWorkspaceMemberByDomain(string domain, string userId)
        {

            // get the first workspace with the domain
            var workspace = await _context.WorkspaceDomain.FirstOrDefaultAsync(d => d.Domain == domain);

            if (workspace == null)
            {
                return null!;
            }

            var workspaceMember = new WorkspaceMember
            {
                WorkspaceId = workspace.WorkspaceId,
                ApplicationUserId = userId
            };

            _context.WorkspaceMember.Add(workspaceMember);
            await _context.SaveChangesAsync();

            return workspaceMember;
        }

        public async Task<Workspace> CreateWorkspace(Workspace workspace, string userId)
        {
            workspace.CreatedBy = userId;
            workspace.ModifiedBy = userId;
            workspace.CreatedOn = DateTime.Now;
            workspace.ModifiedOn = DateTime.Now;
            workspace.IsDeleted = false;

            _context.Workspace.Add(workspace);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WorkspaceExists(workspace.Id))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            // Add the creator as a workspace member with Owner role
            await AddWorkspaceMember(workspace.Id!, userId, MemberRole.Owner);

            return workspace;
        }



        //public async Task<Workspace> PutWorkspace(string id, Workspace Workspace)
        //{
        //    if (id != Workspace.Id)
        //    {
        //        return null;
        //    }

        //    _context.Entry(Workspace).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!WorkspaceExists(id))
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return Workspace;
        //}




        //public async Task<Workspace> DeleteWorkspace(string id)
        //{
        //    var Workspace = await _context.Workspace.FindAsync(id);

        //    if (Workspace == null)
        //    {
        //        return null;
        //    }

        //    _context.Workspace.Remove(Workspace);
        //    await _context.SaveChangesAsync();

        //    return null;
        //}




        private bool UserIsMember(string workspaceId, string userId)
        {
            return _context.WorkspaceMember.Any(m => m.WorkspaceId == workspaceId && m.ApplicationUserId == userId);
        }

        private bool WorkspaceExists(string id)
        {
            return _context.Workspace.Any(e => e.Id == id);
        }
        

        private bool WorkspaceMemberExists(string workspaceId, string userId)
        {
            return _context.WorkspaceMember.Any(m => m.WorkspaceId == workspaceId && m.ApplicationUserId == userId);
        }
    }
}
