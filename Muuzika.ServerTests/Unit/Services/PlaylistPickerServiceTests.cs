using Moq;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Interfaces;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Services;
using Muuzika.ServerTests.Helpers.Fakers;

namespace Muuzika.ServerTests.Unit.Services;

public class PlaylistPickerServiceTests
{
    private PlaylistPickerService _playlistPickerService = null!;
    
    private Mock<IPlaylist> _playlistMock = null!;

    [SetUp]
    public void Setup()
    {
        var randomProviderMock = new Mock<IRandomProvider>();
        randomProviderMock.Setup(x => x.GetRandom()).Returns(new Random(42));
        
        _playlistPickerService = new PlaylistPickerService(randomProviderMock.Object);
        
        _playlistMock = new Mock<IPlaylist>();
    }
    
    [Test]
    public void PickOptionsShouldThrowExceptionWhenNotEnoughSongsToPlay()
    {
        var songsNotPlayed = SongFaker.GetSongs(1);
        
        _playlistMock.Setup(x => x.SongsNotPlayed).Returns(songsNotPlayed);
        
        var exception = Assert.Throws<Exception>(() => _playlistPickerService.PickOptions(_playlistMock.Object, 4));
        
        Assert.That(exception?.Message, Is.EqualTo("Not enough not-played songs"));
    }

    [Test]
    public void PickOptionsShouldWork()
    {
        const int maxNumberOfOptions = 4;

        var songsNotPlayed = SongFaker.GetSongs();

        _playlistMock.Setup(x => x.SongsNotPlayed).Returns(songsNotPlayed);

        var options = _playlistPickerService.PickOptions(_playlistMock.Object, maxNumberOfOptions);
        
        /*
         * I'm having trouble asserting exact ids here, even when using a seed or mocking Random.Next()
         * This probably happens because of the way that LINQ can be executed in a non-sequence way
         */
        Assert.Multiple(() =>
        {
            Assert.That(options, Is.Unique);
            Assert.That(options, Has.Length.EqualTo(maxNumberOfOptions));
        });
    }
    
    [Test]
    public void PickOptionsAvoidingRepeatedArtistsShouldThrowExceptionWhenNotEnoughArtistsWithSongsToPlay()
    {
        var artistsWithSongsNotPlayed = ArtistFaker.GetArtists(1);
        
        _playlistMock.Setup(x => x.ArtistsWithSongsNotPlayed).Returns(artistsWithSongsNotPlayed);
        
        var exception = Assert.Throws<Exception>(() => _playlistPickerService.PickOptionsAvoidingRepeatedArtists(_playlistMock.Object, 4));
        
        Assert.That(exception?.Message, Is.EqualTo("Not enough artists with not-played songs"));
    }

    [Test]
    public void PickOptionsAvoidingRepeatedArtistsShouldWork()
    {
        const int maxNumberOfOptions = 4;
        var artistsWithSongsNotPlayed = ArtistFaker.GetArtists(5);

        _playlistMock.Setup(x => x.ArtistsWithSongsNotPlayed).Returns(artistsWithSongsNotPlayed);
        _playlistMock
            .Setup(x => x.GetSongsNotPlayedFromArtist(It.IsAny<Artist>()))
            .Returns((Artist artist) => SongFaker.GetSongs(artist));
        
        var options = _playlistPickerService.PickOptionsAvoidingRepeatedArtists(_playlistMock.Object, maxNumberOfOptions);

        var artists = options.Select(x => x.Artists.First());
        
        /*
         * I'm having trouble asserting exact ids here, even when using a seed or mocking Random.Next()
         * This probably happens because of the way that LINQ can be executed in a non-sequence way
         */
        Assert.Multiple(() =>
        {
            Assert.That(options, Is.Unique);
            Assert.That(options, Has.Length.EqualTo(maxNumberOfOptions));
            Assert.That(artists, Is.Unique);
        });
    }
        
    
}