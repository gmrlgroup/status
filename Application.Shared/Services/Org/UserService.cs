using Application.Shared.Data;
using Application.Shared.Models;
using Application.Shared.Models.User;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text;
using System.Linq;

namespace Application.Shared.Services.Org
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IWorkspaceService _workspaceService;

        public UserService(ApplicationDbContext context,
                            UserManager<ApplicationUser> userManager,
                            IUserStore<ApplicationUser> userStore,
                            IWorkspaceService workspaceService)
        {
            _context = context;
            _userManager = userManager;
            _userStore = userStore;
            _workspaceService = workspaceService;
        }


        public async Task<List<ApplicationUser>> GetUsers(string workspaceId)
        {
            var applicationUserMembersList = await _context.WorkspaceMember.Where(m => m.WorkspaceId == workspaceId).ToListAsync();
            var applicationUserMembers = applicationUserMembersList.Select(m => m.ApplicationUserId).ToArray();

            return await _context.ApplicationUser.Where(c => applicationUserMembers.Contains(c.Id)).ToListAsync();
        }


        public async Task<ApplicationUser> GetUser(string id)
        {
            return await _context.ApplicationUser.FindAsync(id);
        }

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<List<string>> GetUseremails(string workspaceId)
        {
            var members = await _context.WorkspaceMember.Where(m => m.WorkspaceId == workspaceId).ToListAsync();

            var userIds = members.Select(m => m.ApplicationUserId).ToArray();

            var users = await _context.ApplicationUser.Where(u => userIds.Contains(u.Id)).ToListAsync();

            return users.Select(u => u.Email).ToList();
        }



        private IEnumerable<IdentityError>? identityErrors;

        public async Task<ApplicationUser> RegisterUser(UserInputModel userInput, string workspaceId)
        {
            var user = CreateUser();
            user.EmailConfirmed = true;

            await _userStore.SetUserNameAsync(user, userInput.UserName, CancellationToken.None);
            var emailStore = GetEmailStore();
            await emailStore.SetEmailAsync(user, userInput.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, userInput.Password);

            if (!result.Succeeded)
            {
                identityErrors = result.Errors;

                Console.WriteLine("Error: " + result.Errors);

                return null;
            }


            // add the user as a member to the workspace
            await _workspaceService.AddWorkspaceMember(workspaceId, user.Id);


            var userId = await _userManager.GetUserIdAsync(user);

            user.Id = userId;


            return user;


        }


        // Update
        public async Task<ApplicationUser> UpdateUserAsync(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return user;
            }

            return null;
        }

        // Delete
        public async Task<ApplicationUser> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return null;
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return user;
            }

            return null;
        }

        // change password
        public async Task<ApplicationUser> ChangePasswordAsync(string id, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return null;
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                return user;
            }

            return null;
        }

        public async Task<Workspace> CreateWorkspaceForUser(string userEmail, string userId)
        {
            // Generate workspace name from email (remove @domain.com)
            var workspaceName = userEmail.Split('@')[0];

            // Generate a unique short workspace ID (max 10 characters)
            var workspaceId = GenerateWorkspaceId();
            
            // Ensure the workspace ID is unique
            try
            {
                while (await _workspaceService.GetWorkspace(workspaceId) != null)
                {
                    workspaceId = GenerateWorkspaceId();
                }
            }
            catch
            {
                // If GetWorkspace throws an exception (workspace not found), the ID is unique
            }

            // Create the workspace
            var workspace = new Workspace
            {
                Id = workspaceId,
                Name = workspaceName
            };

            // Use WorkspaceService to create workspace and add user as Owner
            var createdWorkspace = await _workspaceService.CreateWorkspace(workspace, userId);
            
            return createdWorkspace;
        }

        private string GenerateWorkspaceId()
        {
            // Generate a short random string (max 10 characters)
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }


        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
            }
        }


    }
}
