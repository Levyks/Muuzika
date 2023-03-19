using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Muuzika.Gateway.Services.Interfaces;

namespace Muuzika.Gateway.Services;

public class JwtService : IJwtService
{
    private readonly JwtSecurityTokenHandler _handler;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _algorithm = SecurityAlgorithms.HmacSha256Signature;
    private readonly int _defaultTokenExpirationInMinutes;

    public JwtService(IConfiguration configuration, JwtSecurityTokenHandler handler)
    {
        _handler = handler;
        var keyString = configuration["Jwt:Key"] ??
                        throw new ArgumentNullException(nameof(configuration), "Jwt:Key is not defined");
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        _issuer = configuration["Jwt:Issuer"] ??
                  throw new ArgumentNullException(nameof(configuration), "Jwt:Issuer is not defined");
        _audience = configuration["Jwt:Audience"] ??
                    throw new ArgumentNullException(nameof(configuration), "Jwt:Audience is not defined");
        _defaultTokenExpirationInMinutes = configuration.GetValue("Jwt:DefaultTokenExpirationInMinutes", 60);
    }

    public JwtService(IConfiguration configuration)
        : this(configuration, new JwtSecurityTokenHandler())
    {
    }

    public string GenerateToken(ClaimsIdentity identity, DateTime? tokenExpiresAt = null)
    {
        tokenExpiresAt ??= DateTime.UtcNow.AddMinutes(_defaultTokenExpirationInMinutes);

        var handler = new JwtSecurityTokenHandler();

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Expires = tokenExpiresAt,
            SigningCredentials = new SigningCredentials(_securityKey, _algorithm),
            Issuer = _issuer,
            Audience = _audience
        };

        var token = handler.CreateToken(descriptor);

        return handler.WriteToken(token);
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