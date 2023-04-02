using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Services;

public class JwtService : IJwtService
{
    private readonly JwtSecurityTokenHandler _handler;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly string _issuer;
    private readonly string _audience;
    
    private const string Algorithm = SecurityAlgorithms.HmacSha256Signature;

    public JwtService(
        IConfigProvider configProvider,
        JwtSecurityTokenHandler handler,
        IDateTimeProvider dateTimeProvider)
    {
        _handler = handler;
        _dateTimeProvider = dateTimeProvider;
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configProvider.JwtKey));
        _issuer = configProvider.JwtIssuer;
        _audience = configProvider.JwtAudience;
    }

    public string GenerateToken(ClaimsIdentity identity, Func<DateTime, DateTime> getTokenExpiresAt)
    {
        var now = _dateTimeProvider.GetNow();

        var descriptor = new SecurityTokenDescriptor
        {
            NotBefore = now,
            Expires = getTokenExpiresAt(now),
            IssuedAt = now,
            SigningCredentials = new SigningCredentials(_securityKey, Algorithm),
            Issuer = _issuer,
            Audience = _audience,
            Subject = identity
        };

        var token = _handler.CreateToken(descriptor);

        return _handler.WriteToken(token);
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token, out SecurityToken? validatedToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _securityKey,
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true
        };

        try
        {
            return _handler.ValidateToken(token, tokenValidationParameters, out validatedToken);
        }
        catch
        {
            validatedToken = null;
            return null;
        }
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        return GetPrincipalFromToken(token, out _);
    }
} 