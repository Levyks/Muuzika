using Microsoft.Extensions.DependencyInjection;
using Muuzika.Application.Helpers;
using Muuzika.Application.Helpers.Interfaces;
using Muuzika.Application.Playlist;
using Muuzika.Application.Playlist.Interfaces;

namespace Muuzika.Application;

public static class DependencyInjection
{
    public static void RegisterApplication(this IServiceCollection services)
    {
        services.AddSingleton<IRandomHelper, RandomHelper>();
        services.AddTransient<IPlaylistWrapper, PlaylistWrapper>();
    }
}