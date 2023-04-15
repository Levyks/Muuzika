using System.Collections.Immutable;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Models;
using Muuzika.ServerTests.Helpers.Fakers;

namespace Muuzika.ServerTests.Unit.Models;

public class PlaylistTests
{
    [Test]
    public void StringValuesFromConstructorShouldBeStoredCorrectly()
    {
        const string id = "id";
        const string name = "name";
        const string createdBy = "createdBy";
        const string url = "url";
        const string imageUrl = "imageUrl";
        
        var playlist = new Playlist(SongProvider.Spotify, id, name, createdBy, url, imageUrl, ImmutableArray<Song>.Empty);
        
        Assert.Multiple(() =>
        {
            Assert.That(playlist.Id, Is.EqualTo(id));
            Assert.That(playlist.Name, Is.EqualTo(name));
            Assert.That(playlist.CreatedBy, Is.EqualTo(createdBy));
            Assert.That(playlist.Url, Is.EqualTo(url));
            Assert.That(playlist.ImageUrl, Is.EqualTo(imageUrl));
        });
    }
    
    [Test]
    public void NotPlayedCountsShouldBeCorrect()
    {
        const int numberOfDifferentArtists = 10;
        int[] numberOfSongsPerArtist = { 1, 5, 1, 3, 1, 3, 4, 5, 6, 1 };
        
        var songs = SongFaker.GetSongs(numberOfDifferentArtists, numberOfSongsPerArtist);
        
        var playlist = new Playlist(SongProvider.Spotify, "id", "name", "createdBy", "url", "imageUrl", songs);
        
        Assert.Multiple(() =>
        {
            Assert.That(playlist.Songs.Count(), Is.EqualTo(30));
            Assert.That(playlist.SongsNotPlayed.Count(), Is.EqualTo(30));
            Assert.That(playlist.ArtistsWithSongsNotPlayed.Count(), Is.EqualTo(10));
        });

        var indexesWithExpectedNumberOfSongs = Enumerable.Range(0, numberOfDifferentArtists)
            .Zip(numberOfSongsPerArtist, (i, numberOfSongs) => (i, numberOfSongs));

        foreach (var (i, expectedSongsNotPlayedCount) in indexesWithExpectedNumberOfSongs)
        {
            var artist = new Artist(SongProvider.Spotify, $"id-{i}", $"name-{i}", $"url-{i}");
            Assert.That(playlist.GetSongsNotPlayedFromArtist(artist).Count(), Is.EqualTo(expectedSongsNotPlayedCount));
        }
    }

    [Test]
    public void ShouldCalculateCorrectNumberOfNumberOfPlayableSongRounds()
    {
        var songs = SongFaker.GetSongs(10, new []{ 1, 5, 1, 3, 1, 3, 4, 5, 6, 1 });
        
        var playlist = new Playlist(SongProvider.Spotify, "id", "name", "createdBy", "url", "imageUrl", songs);
        
        Assert.That(playlist.NumberOfPlayableSongRounds, Is.EqualTo(29));
    }
    
    [Test]
    public void ShouldCalculateCorrectNumberOfNumberOfPlayableArtistRounds()
    {
        var songs = SongFaker.GetSongs(5, new []{ 1, 5, 1, 3, 1 });
        
        var playlist = new Playlist(SongProvider.Spotify, "id", "name", "createdBy", "url", "imageUrl", songs);
        
        Assert.That(playlist.NumberOfPlayableArtistRounds, Is.EqualTo(6));
    }

    [Test]
    public void MarkSongAsPlayedShouldDecreaseSongsNotPlayedCountTotalAndForArtist()
    {
        var songs = SongFaker.GetSongs();
        
        var playlist = new Playlist(SongProvider.Spotify, "id", "name", "createdBy", "url", "imageUrl", songs);
        
        var song = playlist.SongsNotPlayed.Last();
        
        var songsNotPlayedCount = playlist.SongsNotPlayed.Count();
        var songsNotPlayedFromArtistCount = playlist.GetSongsNotPlayedFromArtist(song.Artists.First()).Count();
        
        playlist.MarkSongAsPlayed(song);
        Assert.Multiple(() =>
        {
            Assert.That(playlist.SongsNotPlayed.Count(), Is.EqualTo(songsNotPlayedCount - 1));
            Assert.That(playlist.GetSongsNotPlayedFromArtist(song.Artists.First()).Count(), Is.EqualTo(songsNotPlayedFromArtistCount - 1));
        });
    }
    
    [Test]
    public void MarkSongAsPlayedShouldDecreaseSongsNotPlayedCountTotalAndRemoveArtistFromListOfArtistsWithSongsNotPlayedIfItIsTheLastOne()
    {
        var songs = SongFaker.GetSongs();
        
        var playlist = new Playlist(SongProvider.Spotify, "id", "name", "createdBy", "url", "imageUrl", songs);
        
        var song = playlist.SongsNotPlayed.First();
        
        var songsNotPlayedCount = playlist.SongsNotPlayed.Count();
        
        playlist.MarkSongAsPlayed(song);
        Assert.Multiple(() =>
        {
            Assert.That(playlist.SongsNotPlayed.Count(), Is.EqualTo(songsNotPlayedCount - 1));
            Assert.That(playlist.ArtistsWithSongsNotPlayed, Does.Not.Contain(song.Artists.First()));
        });
    }
}