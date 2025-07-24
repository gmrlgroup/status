using Application.Shared.Services.Org;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;


namespace Application.Attributes;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ValidateWorkspaceMembershipAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var headers = context.HttpContext.Request.Headers;

        if (!headers.ContainsKey("X-Workspace-ID") || string.IsNullOrEmpty(headers["X-Workspace-ID"]))
        {
            context.Result = new BadRequestObjectResult("Workspace should be in the header");
            return;
        }

        if (!headers.ContainsKey("userId") || string.IsNullOrEmpty(headers["userId"]))
        {
            context.Result = new BadRequestObjectResult("User should be in the header");
            return;
        }

        var workspaceId = headers["X-Workspace-ID"].ToString();
        var userId = headers["userId"].ToString();

        // Resolve the service dynamically to allow DI
        var workspaceService = context.HttpContext.RequestServices.GetService(typeof(IWorkspaceService)) as IWorkspaceService;
        
        if (workspaceService == null)
        {
            context.Result = new StatusCodeResult(500); // Internal server error if service is missing
            return;
        }

        bool userIsWorkspaceMember = await workspaceService.UserIsWorkspaceMember(userId, workspaceId);

        if (!userIsWorkspaceMember)
        {
            context.Result = new UnauthorizedObjectResult($"You are not a member of the workspace {workspaceId}");
            return;
        }

        // Proceed to the next action
        await next();
    }
}
