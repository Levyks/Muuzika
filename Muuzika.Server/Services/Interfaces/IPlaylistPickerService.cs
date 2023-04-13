using System.Collections.Immutable;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Interfaces;

namespace Muuzika.Server.Services.Interfaces;

public interface IPlaylistPickerService
{
    ImmutableArray<Song> PickOptions(IPlaylist playlist, int maxNumberOfOptions);
    
    ImmutableArray<Song> PickOptionsAvoidingRepeatedArtists(IPlaylist playlist, int maxNumberOfOptions);
}