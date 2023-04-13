namespace Muuzika.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Reuse<TInterface>(this IServiceCollection serviceCollection, IServiceProvider serviceProvider) where TInterface : class
    {
        var service = serviceProvider.GetRequiredService<TInterface>();
        serviceCollection.AddSingleton(service);
        return serviceCollection;
    }
}