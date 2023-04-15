using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.ServerTests.E2E.Helpers;

public abstract class BaseE2ETest
{
    protected internal DateTime Now;
    protected internal Mock<IRandomProvider> RandomProviderMock = null!;
    protected internal Mock<IDateTimeProvider> DateTimeProviderMock = null!;
    protected internal IConfiguration Configuration = null!;

    protected internal JsonSerializerOptions JsonSerializerOptions = null!;
    protected internal IExceptionMapper ExceptionMapper = null!;
    
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
        
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.e2e.json")
            .Build();
        
        Factory = new MockableMuuzikaWebApplicationFactory()
            .Mock(RandomProviderMock)
            .Mock(Configuration)
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