using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Muuzika.Gateway.Providers.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Services;

public class JwtService : IJwtService
{
    private readonly JwtSecurityTokenHandler _handler;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly string _issuer;
    private readonly string _audience;
    
    private const string Algorithm = SecurityAlgorithms.HmacSha256Signature;

    public JwtService(
        string? key,
        string? issuer,
        string? audience,
        JwtSecurityTokenHandler handler)
    {
        _handler = handler;
        if (key == null) throw new ArgumentNullException(nameof(key));
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        _audience = audience ?? throw new ArgumentNullException(nameof(audience));
    }

    public JwtService(IConfiguration configuration, IDateTimeProvider dateTimeProvider, JwtSecurityTokenHandler handler): 
        this (configuration["Jwt:Key"], 
            configuration["Jwt:Issuer"], 
            configuration["Jwt:Audience"], 
            handler)
    {
    }

    public JwtService(IConfiguration configuration, IDateTimeProvider dateTimeProvider): 
        this(configuration, dateTimeProvider, new JwtSecurityTokenHandler())
    {
    }

    public string GenerateToken(ClaimsIdentity identity, DateTime? tokenExpiresAt)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            NotBefore = DateTime.MinValue,
            Expires = tokenExpiresAt,
            SigningCredentials = new SigningCredentials(_securityKey, Algorithm),
            Issuer = _issuer,
            Audience = _audience
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
} 