using Moq;
using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.ServerTests.E2E.Helpers;

public abstract class BaseE2ETest
{
    private DateTime _now;
    private Mock<IConfigProvider> _configProviderMock = null!;
    private Mock<IDateTimeProvider> _dateTimeProviderMock = null!;
    
    private const string JwtKey = "f4r0u71n7h3unch4r73db4ckw473r5";
    private const string JwtIssuer = "https://muuzika.com";
    private const string JwtAudience = "https://muuzika.com";
    
    internal MockableMuuzikaWebApplicationFactory Factory = null!;
    private HttpClient? _client;
    internal HttpClient Client => _client ??= Factory.CreateClient();

    [SetUp]
    public void Setup()
    {
        _now = DateTime.UtcNow;

        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.GetNow()).Returns(_now);
        
        _configProviderMock = new Mock<IConfigProvider>();
        _configProviderMock.Setup(x => x.JwtKey).Returns(JwtKey);
        _configProviderMock.Setup(x => x.JwtIssuer).Returns(JwtIssuer);
        _configProviderMock.Setup(x => x.JwtAudience).Returns(JwtAudience);
        _configProviderMock.Setup(x => x.DelayCloseRoomAfterLastPlayerLeft).Returns(TimeSpan.FromMinutes(5));
        _configProviderMock.Setup(x => x.DelayDisconnectedPlayerRemoval).Returns(TimeSpan.FromMinutes(2));
        
        Factory = new MockableMuuzikaWebApplicationFactory()
            .Mock(() => new Random(42))
            .Mock(_configProviderMock)
            .Mock(_dateTimeProviderMock);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await Factory.DisposeAsync();
        Factory = null!;
        _client = null;
    }
}