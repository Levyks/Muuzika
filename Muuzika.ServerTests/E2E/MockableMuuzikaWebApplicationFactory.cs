using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Muuzika.Server;

namespace Muuzika.ServerTests.E2E;

public class MockableMuuzikaWebApplicationFactory: WebApplicationFactory<Startup>
{
    private readonly List<Tuple<Type, object>> _serviceOverrides = new();

    public MockableMuuzikaWebApplicationFactory Mock<TInterface>(TInterface implementation) where TInterface : class
    {
        _serviceOverrides.Add(new Tuple<Type, object>(typeof(TInterface), implementation));
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