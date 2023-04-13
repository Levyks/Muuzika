using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Muuzika.Server.Enums.Room;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Providers;
using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.ServerTests.E2E.Helpers;

public abstract class BaseE2ETest
{
    internal DateTime Now;
    internal Mock<IRandomProvider> RandomProviderMock = null!;
    internal Mock<IConfigProvider> ConfigProviderMock = null!;
    internal Mock<IDateTimeProvider> DateTimeProviderMock = null!;

    internal JsonSerializerOptions JsonSerializerOptions = null!;
    internal IExceptionMapper ExceptionMapper = null!;
    
    internal MockableMuuzikaWebApplicationFactory Factory = null!;
    private HttpClient? _client;
    internal HttpClient Client => _client ??= Factory.CreateClient();

    [SetUp]
    public void Setup()
    {
        Now = DateTime.UtcNow;
        
        RandomProviderMock = new Mock<IRandomProvider>();
        RandomProviderMock.Setup(x => x.GetRandom()).Returns(new Random(42));

        DateTimeProviderMock = new Mock<IDateTimeProvider>();
        DateTimeProviderMock.Setup(x => x.GetNow()).Returns(Now);
        
        ConfigProviderMock = new Mock<IConfigProvider>();
        ConfigProviderMock.Setup(x => x.JwtKey).Returns("f4r0u71n7h3unch4r73db4ckw473r5");
        ConfigProviderMock.Setup(x => x.JwtIssuer).Returns("https://muuzika.com");
        ConfigProviderMock.Setup(x => x.JwtAudience).Returns("https://muuzika.com");
        
        ConfigProviderMock.Setup(x => x.DelayCloseRoomAfterLastPlayerLeft).Returns(TimeSpan.FromMinutes(5));
        ConfigProviderMock.Setup(x => x.DelayDisconnectedPlayerRemoval).Returns(TimeSpan.FromMinutes(2));
        
        ConfigProviderMock.Setup(x => x.RoomDefaultPossibleRoundTypes).Returns(RoomPossibleRoundTypes.Both);
        ConfigProviderMock.Setup(x => x.RoomDefaultRoundsCount).Returns(5);
        ConfigProviderMock.Setup(x => x.RoomDefaultRoundDuration).Returns(TimeSpan.FromSeconds(15));
        
        Factory = new MockableMuuzikaWebApplicationFactory()
            .Mock(RandomProviderMock)
            .Mock(ConfigProviderMock)
            .Mock(DateTimeProviderMock);
        
        JsonSerializerOptions = Factory.Services.GetRequiredService<JsonSerializerOptions>();
        ExceptionMapper = Factory.Services.GetRequiredService<IExceptionMapper>();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await Factory.DisposeAsync();
        Factory = null!;
        _client = null;
    }
}