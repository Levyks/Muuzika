using Microsoft.AspNetCore.SignalR.Client;
using Muuzika.Server.Enums.Room;
using Muuzika.Server.Models;
using Muuzika.ServerTests.E2E.Helpers;
using Muuzika.ServerTests.E2E.Helpers.Extensions;

namespace Muuzika.ServerTests.E2E;

[TestFixture]
public class RoomOptionsE2ETests: BaseE2ETest
{
    [Test]
    public async Task LeaderShouldBeAbleToChangeOptionsAndOtherPlayersShouldBeNotified()
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");
        
        const string playerUsername = "player1";
        
        var playerConnectedResult = await this.JoinRoomAndConnect(leaderConnectedResult.RoomCode, playerUsername);
        
        var newOptions = new RoomOptions(
            maxPlayersCount: 32,
            possibleRoundTypes: RoomPossibleRoundTypes.Song,
            roundsCount: 3,
            roundDuration: TimeSpan.FromSeconds(20)
        );
        
        var optionsChangedTcs = new TaskCompletionSource<DateTime>();

        playerConnectedResult.HubConnection.On<RoomOptions>("RoomOptionsChanged", options => {
            Assert.Multiple(() =>
            {
                Assert.That(options.MaxPlayersCount, Is.EqualTo(newOptions.MaxPlayersCount));
                Assert.That(options.PossibleRoundTypes, Is.EqualTo(newOptions.PossibleRoundTypes));
                Assert.That(options.RoundsCount, Is.EqualTo(newOptions.RoundsCount));
                Assert.That(options.RoundDuration, Is.EqualTo(newOptions.RoundDuration));
            });
            optionsChangedTcs.SetResult(DateTime.UtcNow);
        });
        
        var changeIssuedAt = DateTime.UtcNow;
        await leaderConnectedResult.HubConnection.InvokeAsync("SetOptions", newOptions);
        
        await optionsChangedTcs.Task;
        
        Assert.That(changeIssuedAt, Is.LessThan(optionsChangedTcs.Task.Result));
    }

    [Test]
    public async Task NewOptionsShouldBeStoredAndSentInNextSyncAll()
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");
        
        var newOptions = new RoomOptions(
            maxPlayersCount: 32,
            possibleRoundTypes: RoomPossibleRoundTypes.Song,
            roundsCount: 3,
            roundDuration: TimeSpan.FromSeconds(20)
        );
        await leaderConnectedResult.HubConnection.InvokeAsync("SetOptions", newOptions);
        
        const string playerUsername = "player1";
        
        var playerConnectedResult = await this.JoinRoomAndConnect(leaderConnectedResult.RoomCode, playerUsername);

        var options = playerConnectedResult.ReceivedStateSync.Room.Options;
        Assert.Multiple(() =>
        {
            Assert.That(options.MaxPlayersCount, Is.EqualTo(newOptions.MaxPlayersCount));
            Assert.That(options.PossibleRoundTypes, Is.EqualTo(newOptions.PossibleRoundTypes));
            Assert.That(options.RoundsCount, Is.EqualTo(newOptions.RoundsCount));
            Assert.That(options.RoundDuration, Is.EqualTo(newOptions.RoundDuration));
        });
    }
}