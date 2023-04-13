using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Models;

namespace Muuzika.Server.Auth;

public class LeaderOnlyRequirement:
    AuthorizationHandler<LeaderOnlyRequirement, HubInvocationContext>,
    IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        LeaderOnlyRequirement requirement,
        HubInvocationContext resource)
    {
        resource.Context.Items.TryGetValue("player", out var playerObj);

        if (playerObj is Player { IsLeader: true })
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, ExceptionType.LeaderOnlyAction.ToString()));
        }

        return Task.CompletedTask;
    }
}