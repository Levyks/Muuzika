using Microsoft.AspNetCore.Authorization;
using Muuzika.Server.Auth;

namespace Muuzika.Server.Attributes.Auth;

public class LeaderOnlyAttribute: AuthorizeAttribute
{
    public LeaderOnlyAttribute(): base(LeaderOnlyPolicy.Name)
    {
    }
}