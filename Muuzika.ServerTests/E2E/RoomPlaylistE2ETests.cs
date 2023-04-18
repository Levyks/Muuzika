using Microsoft.AspNetCore.SignalR.Client;
using Muuzika.Server.Models;
using Muuzika.ServerTests.E2E.Helpers;
using Muuzika.ServerTests.E2E.Helpers.Extensions;

namespace Muuzika.ServerTests.E2E;

public class RoomPlaylistE2ETests: BaseE2ETest
{
    [Test]
    public async Task LeaderShouldBeAbleToChangePlaylistAndOtherPlayersShouldBeNotified()
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");
        
        const string playerUsername = "player1";
        
        var playerConnectedResult = await this.JoinRoomAndConnect(leaderConnectedResult.RoomCode, playerUsername);
        
        var setPlaylistDto = new {
            provider = "Spotify",
            id = "59rP0u00qHyXZHKxZojGSr"
        };
        
        var optionsChangedTcs = new TaskCompletionSource<DateTime>();
        
        var changeIssuedAt = DateTime.UtcNow;
        await leaderConnectedResult.HubConnection.InvokeAsync("SetPlaylist", setPlaylistDto);
        
        await optionsChangedTcs.Task;
        
        Assert.That(changeIssuedAt, Is.LessThan(optionsChangedTcs.Task.Result));
    }
}