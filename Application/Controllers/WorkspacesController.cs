using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Shared.Data;
using Application.Shared.Models;
using Application.Shared.Models.User;
using Microsoft.AspNetCore.Identity;
using Application.Shared.Services.Org;

namespace Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkspacesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkspaceService _workspaceService;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkspacesController(ApplicationDbContext context, IWorkspaceService workspaceService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _workspaceService = workspaceService;
            _userManager = userManager;
        }

        // GET: api/Workspaces
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Workspace>>> GetWorkspaces()
        {
            // get userId from header
            var userId = Request.Headers["UserId"].ToString();

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required in headers");
            }

            var workspaces = await _workspaceService.GetWorkspaces(userId);

            return Ok(workspaces);
        }

        // POST: api/Workspaces
        [HttpPost]
        public async Task<ActionResult<Workspace>> CreateWorkspace(Workspace workspace)
        {
            try
            {
                // get userId from header
                var userId = Request.Headers["UserId"].ToString();

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID is required in headers");
                }

                var createdWorkspace = await _workspaceService.CreateWorkspace(workspace, userId);

                return CreatedAtAction(nameof(GetWorkspaces), new { id = createdWorkspace.Id }, createdWorkspace);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating company: {ex.Message}");
            }
        }

        // POST: api/Workspaces/{workspaceId}/members
        [HttpPost("{workspaceId}/members")]
        public async Task<ActionResult<WorkspaceMember>> AddUserToWorkspace(string workspaceId, AddUserToWorkspaceModel model)
        {
            try
            {
                // get userId from header (current user)
                var currentUserId = Request.Headers["UserId"].ToString();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest("User ID is required in headers");
                }

                // Verify current user is a member of the workspace (optional: check if Owner/Manager)
                var isCurrentUserMember = await _workspaceService.UserIsWorkspaceMember(workspaceId, currentUserId);
                if (!isCurrentUserMember)
                {
                    return Unauthorized("You are not authorized to add members to this workspace");
                }

                // Verify the workspace exists
                var workspace = await _workspaceService.GetWorkspace(workspaceId);
                if (workspace == null)
                {
                    return NotFound("Workspace not found");
                }

                // Verify the user exists
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Check if user is already a member
                var isAlreadyMember = await _workspaceService.UserIsWorkspaceMember(workspaceId, model.UserId);
                if (isAlreadyMember)
                {
                    return BadRequest("User is already a member of this workspace");
                }

                // Add the user to the workspace
                var workspaceMember = await _workspaceService.AddWorkspaceMember(workspaceId, model.UserId, model.Role);
                
                if (workspaceMember == null)
                {
                    return BadRequest("Failed to add user to workspace");
                }

                return CreatedAtAction(nameof(GetWorkspaceMembers), new { workspaceId = workspaceId }, workspaceMember);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error adding user to workspace: {ex.Message}");
            }
        }

        // POST: api/Workspaces/{workspaceId}/members/by-email
        [HttpPost("{workspaceId}/members/by-email")]
        public async Task<ActionResult<WorkspaceMember>> AddUserToWorkspaceByEmail(string workspaceId, AddUserToWorkspaceByEmailModel model)
        {
            try
            {
                // get userId from header (current user)
                var currentUserId = Request.Headers["UserId"].ToString();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest("User ID is required in headers");
                }

                // Verify current user is a member of the workspace
                var isCurrentUserMember = await _workspaceService.UserIsWorkspaceMember(workspaceId, currentUserId);
                if (!isCurrentUserMember)
                {
                    return Unauthorized("You are not authorized to add members to this workspace");
                }

                // Verify the workspace exists
                var workspace = await _workspaceService.GetWorkspace(workspaceId);
                if (workspace == null)
                {
                    return NotFound("Workspace not found");
                }

                // Find user by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound($"User with email {model.Email} not found");
                }

                // Check if user is already a member
                var isAlreadyMember = await _workspaceService.UserIsWorkspaceMember(workspaceId, user.Id);
                if (isAlreadyMember)
                {
                    return BadRequest("User is already a member of this workspace");
                }

                // Add the user to the workspace
                var workspaceMember = await _workspaceService.AddWorkspaceMember(workspaceId, user.Id, model.Role);
                
                if (workspaceMember == null)
                {
                    return BadRequest("Failed to add user to workspace");
                }

                return CreatedAtAction(nameof(GetWorkspaceMembers), new { workspaceId = workspaceId }, workspaceMember);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error adding user to workspace: {ex.Message}");
            }
        }

        // GET: api/Workspaces/{workspaceId}/members
        [HttpGet("{workspaceId}/members")]
        public async Task<ActionResult<IEnumerable<WorkspaceMember>>> GetWorkspaceMembers(string workspaceId)
        {
            try
            {
                // get userId from header
                var userId = Request.Headers["UserId"].ToString();

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID is required in headers");
                }

                // Verify current user is a member of the workspace
                var isCurrentUserMember = await _workspaceService.UserIsWorkspaceMember(workspaceId, userId);
                if (!isCurrentUserMember)
                {
                    return Unauthorized("You are not authorized to view members of this workspace");
                }

                var members = await _context.WorkspaceMember
                    .Include(wm => wm.ApplicationUser)
                    .Where(wm => wm.WorkspaceId == workspaceId)
                    .ToListAsync();

                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving workspace members: {ex.Message}");
            }
        }

        // DELETE: api/Workspaces/{workspaceId}/members/{userId}
        [HttpDelete("{workspaceId}/members/{userId}")]
        public async Task<ActionResult> RemoveUserFromWorkspace(string workspaceId, string userId)
        {
            try
            {
                // get current userId from header
                var currentUserId = Request.Headers["UserId"].ToString();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest("User ID is required in headers");
                }

                // Verify current user is a member of the workspace
                var isCurrentUserMember = await _workspaceService.UserIsWorkspaceMember(workspaceId, currentUserId);
                if (!isCurrentUserMember)
                {
                    return Unauthorized("You are not authorized to remove members from this workspace");
                }

                // Check if the user to be removed is a member
                var workspaceMember = await _context.WorkspaceMember
                    .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.ApplicationUserId == userId);

                if (workspaceMember == null)
                {
                    return NotFound("User is not a member of this workspace");
                }

                // Prevent removing the last owner
                var ownerCount = await _context.WorkspaceMember
                    .CountAsync(wm => wm.WorkspaceId == workspaceId && wm.Role == MemberRole.Owner);

                if (workspaceMember.Role == MemberRole.Owner && ownerCount <= 1)
                {
                    return BadRequest("Cannot remove the last owner from the workspace");
                }

                _context.WorkspaceMember.Remove(workspaceMember);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error removing user from workspace: {ex.Message}");
            }
        }

        // PUT: api/Workspaces/{workspaceId}/members/{userId}/role
        [HttpPut("{workspaceId}/members/{userId}/role")]
        public async Task<ActionResult> UpdateMemberRole(string workspaceId, string userId, [FromBody] MemberRole newRole)
        {
            try
            {
                // get current userId from header
                var currentUserId = Request.Headers["UserId"].ToString();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest("User ID is required in headers");
                }

                // Verify current user is a member of the workspace
                var isCurrentUserMember = await _workspaceService.UserIsWorkspaceMember(workspaceId, currentUserId);
                if (!isCurrentUserMember)
                {
                    return Unauthorized("You are not authorized to update member roles in this workspace");
                }

                // Find the workspace member
                var workspaceMember = await _context.WorkspaceMember
                    .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.ApplicationUserId == userId);

                if (workspaceMember == null)
                {
                    return NotFound("User is not a member of this workspace");
                }

                // Prevent removing the last owner
                if (workspaceMember.Role == MemberRole.Owner && newRole != MemberRole.Owner)
                {
                    var ownerCount = await _context.WorkspaceMember
                        .CountAsync(wm => wm.WorkspaceId == workspaceId && wm.Role == MemberRole.Owner);

                    if (ownerCount <= 1)
                    {
                        return BadRequest("Cannot change the role of the last owner");
                    }
                }

                workspaceMember.Role = newRole;
                _context.Entry(workspaceMember).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating member role: {ex.Message}");
            }
        }
    }
}
