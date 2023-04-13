using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Muuzika.Server;

namespace Muuzika.ServerTests.E2E.Helpers;

internal class MockableMuuzikaWebApplicationFactory: WebApplicationFactory<Program>
{
    private readonly List<Tuple<Type, object>> _serviceOverrides = new();

    public MockableMuuzikaWebApplicationFactory Mock<TInterface>(TInterface implementation) where TInterface : class
    {
        _serviceOverrides.Add(new Tuple<Type, object>(typeof(TInterface), implementation));
        return this;
    }
    
    public MockableMuuzikaWebApplicationFactory Mock<TInterface>(Mock<TInterface> implementationMock) where TInterface : class
    {
        _serviceOverrides.Add(new Tuple<Type, object>(typeof(TInterface), implementationMock.Object));
        return this;
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            foreach (var (serviceType, implementation) in _serviceOverrides)
            {
                services.AddSingleton(serviceType, implementation);
            }
        });
    }
}