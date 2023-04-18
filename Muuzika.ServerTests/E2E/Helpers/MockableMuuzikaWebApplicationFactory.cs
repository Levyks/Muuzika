using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Muuzika.Server;

namespace Muuzika.ServerTests.E2E.Helpers;

internal class MockableMuuzikaWebApplicationFactory: WebApplicationFactory<Program>
{
    private readonly List<Tuple<Type, object>> _singletonOverrides = new();
    private readonly List<Tuple<Type, Func<IServiceProvider, object>>> _transientOverrides = new();

    public MockableMuuzikaWebApplicationFactory Mock<TInterface>(TInterface implementation) where TInterface : class
    {
        _singletonOverrides.Add(new Tuple<Type, object>(typeof(TInterface), implementation));
        return this;
    }
    
    public MockableMuuzikaWebApplicationFactory Mock<TInterface>(Mock<TInterface> implementationMock) where TInterface : class
    {
        return Mock(implementationMock.Object);
    }
    
    public MockableMuuzikaWebApplicationFactory MockTransient<TInterface>(Func<IServiceProvider, TInterface> implementationFactory) where TInterface : class
    {
        _transientOverrides.Add(new Tuple<Type, Func<IServiceProvider, object>>(typeof(TInterface), implementationFactory));
        return this;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            foreach (var (serviceType, implementationFactory) in _transientOverrides)
            {
                services.AddTransient(serviceType, implementationFactory);
            }
            foreach (var (serviceType, implementation) in _singletonOverrides)
            {
                services.AddSingleton(serviceType, implementation);
            }
        });
    }
}