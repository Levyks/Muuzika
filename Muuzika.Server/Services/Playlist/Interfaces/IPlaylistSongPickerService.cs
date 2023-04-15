using System.Collections.Immutable;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Interfaces;

namespace Muuzika.Server.Services.Playlist.Interfaces;

public interface IPlaylistSongPickerService
{
    ImmutableArray<Song> PickSongs(IPlaylist playlist, int maxNumberOfSongs);
    
    ImmutableArray<Song> PickSongsFromDifferentArtists(IPlaylist playlist, int maxNumberOfSongs);
}