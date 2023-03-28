using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Muuzika.Server.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(ClaimsIdentity identity, DateTime? tokenExpiresAt = null);

    ClaimsPrincipal? GetPrincipalFromToken(string token, out SecurityToken? validatedToken);
}