using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Muuzika.Server.Dtos.Gateway;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Muuzika.Gateway.Providers.Interfaces;
using Muuzika.Server.Services;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.ServerTests.E2E;

[TestFixture]
public class RoomE2ETests
{
    private MockableMuuzikaWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    
    private Mock<IRandomService> _randomServiceMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<IDateTimeProvider> _dateTimeProviderMock = null!;
    
    private DateTime _now;

    private const string JwtKey = "f4r0u71n7h3unch4r73db4ckw473r5";
    private const string JwtIssuer = "https://muuzika.com";
    private const string JwtAudience = "https://muuzika.com";
    
    [SetUp]
    public void Setup()
    {
        _now = new DateTime(2021, 1, 5);
        
        _randomServiceMock = new Mock<IRandomService>();
        
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.GetNow()).Returns(_now);
        
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["Jwt:Key"]).Returns(JwtKey);
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns(JwtIssuer);
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns(JwtAudience);
        
        _factory = new MockableMuuzikaWebApplicationFactory()
            .Mock(_randomServiceMock.Object)
            .Mock(_dateTimeProviderMock.Object)
            .Mock(_configurationMock.Object);
        
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task TestCreateRoom()
    {
        const string username = "test";
        const string roomCode = "1234";

        _randomServiceMock
            .Setup(x => x.GenerateRandomNumericString(4))
            .Returns(roomCode);
        
        var response = await _client.PostAsJsonAsync("/room", new UsernameDto(username));
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var content = await response.Content.ReadAsStringAsync();

        var contentObject = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
        
        Assert.That(contentObject, Is.Not.Null);
        if (contentObject == null) return;

        var tokenElement = contentObject.GetValueOrDefault("token") as JsonElement?;
        Assert.That(tokenElement?.ValueKind, Is.EqualTo(JsonValueKind.String));
        
        var token = tokenElement?.GetString();
        if (token == null) return;

        Assert.Multiple(() =>
        {
            var usernameElement = contentObject.GetValueOrDefault("username") as JsonElement?;
            Assert.That(usernameElement?.GetString(), Is.EqualTo(username));
            
            var roomCodeElement = contentObject.GetValueOrDefault("roomCode") as JsonElement?;
            Assert.That(roomCodeElement?.GetString(), Is.EqualTo(roomCode));
        });
        

        var jwtService = new JwtService(JwtKey, JwtIssuer, JwtAudience, new JwtSecurityTokenHandler());
        
        SecurityToken? securityToken = null;
        var principal = jwtService.GetPrincipalFromToken(token, out securityToken);
        Assert.Multiple(() =>
        {
            Assert.That(principal, Is.Not.Null);
            Assert.That(securityToken, Is.Not.Null);
        });
        if (principal == null || securityToken == null) return;
        
        Assert.Multiple(() =>
        {
            Assert.That(principal.Claims, Has.Exactly(1).Property("Type").EqualTo("roomCode").And.Property("Value").EqualTo(roomCode));
            Assert.That(principal.Claims, Has.Exactly(1).Property("Type").EqualTo("username").And.Property("Value").EqualTo(username));
        });
    }
}