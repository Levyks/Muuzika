using Microsoft.AspNetCore.Authorization;

namespace Muuzika.Server.Auth;

public class LeaderOnlyPolicy: AuthorizationPolicy
{
    public static string Name => "LeaderOnly";

    private LeaderOnlyPolicy() : base(new [] { new LeaderOnlyRequirement()}, Array.Empty<string>())
    {
    }
    
    public static void Register(AuthorizationOptions options) => options.AddPolicy(Name, new LeaderOnlyPolicy());
}