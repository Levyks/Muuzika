using Models = Muuzika.Domain.Models;
using Muuzika.Domain.Enums;
using Muuzika.Domain.Models.Playlist;


namespace Muuzika.Application.Playlist.Interfaces;

public interface IPlaylistWrapper
{
    Models.Playlist.Playlist Playlist { get; }
    int NumberOfSongs { get; }
    int NumberOfPlayedSongs { get; }
    
    int GetNumberOfPlayableRounds(RoundType roundType);
    (Song, ICollection<Song>) GetSongAndChoices(RoundType roundType, int maxNumberOfChoices);

    void Load(Models.Playlist.Playlist playlist);
    void Reset();
}