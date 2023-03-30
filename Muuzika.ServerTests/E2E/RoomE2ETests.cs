using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Muuzika.Server.Dtos.Gateway;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Services;

namespace Muuzika.ServerTests.E2E;

[TestFixture]
public class RoomE2ETests
{
    private MockableMuuzikaWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<IDateTimeProvider> _dateTimeProviderMock = null!;
    
    private DateTime _now;

    private const string JwtKey = "f4r0u71n7h3unch4r73db4ckw473r5";
    private const string JwtIssuer = "https://muuzika.com";
    private const string JwtAudience = "https://muuzika.com";
    
    [SetUp]
    public void Setup()
    {
        _now = DateTime.UtcNow;

        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.GetNow()).Returns(_now);
        
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["Jwt:Key"]).Returns(JwtKey);
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns(JwtIssuer);
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns(JwtAudience);
        
        _factory = new MockableMuuzikaWebApplicationFactory()
            .Mock(() => new Random(42))
            .Mock(_configurationMock.Object)
            .Mock(_dateTimeProviderMock.Object);
        
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task TestCreateRoom()
    {
        const string username = "test";
        const string captchaToken = "foo";
        const string roomCode = "8962";

        var body = new CreateOrJoinRoomDto(username, captchaToken);
        var response = await _client.PostAsJsonAsync("/room", body);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var contentString = await response.Content.ReadAsStringAsync();
        var contentDto = JsonSerializer.Deserialize<RoomCreatedOrJoinedDto>(contentString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Assert.That(contentDto, Is.Not.Null);
        if (contentDto == null) return;
        
        Assert.Multiple(() =>
        {
            Assert.That(contentDto.Token, Is.Not.Null);
            Assert.That(contentDto.Username, Is.EqualTo(username));
            Assert.That(contentDto.RoomCode, Is.EqualTo(roomCode));
        });

        var jwtService = new JwtService(JwtKey, JwtIssuer, JwtAudience, new JwtSecurityTokenHandler(), _dateTimeProviderMock.Object);

        var principal = jwtService.GetPrincipalFromToken(contentDto.Token, out _);
        Assert.That(principal, Is.Not.Null);
        
        if (principal == null) return;
        
        Assert.Multiple(() =>
        {
            Assert.That(principal.Claims, Has.Exactly(1).Property("Type").EqualTo("roomCode").And.Property("Value").EqualTo(roomCode));
            Assert.That(principal.Claims, Has.Exactly(1).Property("Type").EqualTo("username").And.Property("Value").EqualTo(username));
        });
    }
}