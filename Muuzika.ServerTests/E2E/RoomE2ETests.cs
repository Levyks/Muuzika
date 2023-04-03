using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Enums.Room;
using Muuzika.Server.Models;
using Muuzika.ServerTests.E2E.Helpers;
using Muuzika.ServerTests.E2E.Helpers.Extensions;

namespace Muuzika.ServerTests.E2E;

[TestFixture]
public class RoomE2ETests: BaseE2ETest
{
    
    [Test]
    public async Task ShouldCreateRoomAndConnect()
    {
        await this.CreateRoomAndConnect("leader");
    }

    [Test]
    public async Task ShouldJoinAndConnectAndOtherPlayersShouldBeNotified()
    {
        var (roomCode, hubConnection) = await this.CreateRoomAndConnect("leader");

        const string playerUsername = "player1";
        
        var joinedTcs = new TaskCompletionSource<DateTime>();
        hubConnection.On<string>("PlayerJoined", username =>
        {
            Assert.That(username, Is.EqualTo(playerUsername));
            joinedTcs.SetResult(DateTime.UtcNow);
        });
        
        var isConnectedChangedTcs = new TaskCompletionSource<DateTime>();
        hubConnection.On<string, bool>("PlayerIsConnectedChanged", (username, isConnected) => {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.EqualTo(playerUsername));
                Assert.That(isConnected, Is.True);
            });
            isConnectedChangedTcs.SetResult(DateTime.UtcNow);
        });
        
        var joinIssuedAt = DateTime.UtcNow;
        await this.JoinRoomAndConnect(roomCode, playerUsername);
        
        await Task.WhenAll(joinedTcs.Task, isConnectedChangedTcs.Task);
        Assert.Multiple(() =>
        {
            Assert.That(joinIssuedAt, Is.LessThan(joinedTcs.Task.Result));
            Assert.That(joinedTcs.Task.Result, Is.LessThan(isConnectedChangedTcs.Task.Result));
        });
    }

    [Test]
    public async Task ShouldNotifyOtherPlayersWhenOneDisconnects()
    {
        var (roomCode, leaderHubConnection) = await this.CreateRoomAndConnect("leader");

        const string playerUsername = "player1";
        
        var playerHubConnection = await this.JoinRoomAndConnect(roomCode, playerUsername);
        
        var isConnectedChangedTcs = new TaskCompletionSource<DateTime>();
        leaderHubConnection.On<string, bool>("PlayerIsConnectedChanged", (username, isConnected) => {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.EqualTo(playerUsername));
                Assert.That(isConnected, Is.False);
            });
            isConnectedChangedTcs.SetResult(DateTime.UtcNow);
        });
        
        var disconnectedIssuedAt = DateTime.UtcNow;
        await playerHubConnection.StopAsync();
        
        await isConnectedChangedTcs.Task;
        
        Assert.That(disconnectedIssuedAt, Is.LessThan(isConnectedChangedTcs.Task.Result));
    }

    [Test]
    public async Task ShouldMakeOtherPlayerLeaderInCaseHeLeaves()
    {
        var (roomCode, leaderHubConnection) = await this.CreateRoomAndConnect("leader");

        const string playerUsername = "player1";
        
        var playerHubConnection = await this.JoinRoomAndConnect(roomCode, playerUsername);
        
        var leaderChangedTcs = new TaskCompletionSource<DateTime>();
        playerHubConnection.On<string>("RoomLeaderChanged", username => {
            Assert.That(username, Is.EqualTo(playerUsername));
            leaderChangedTcs.SetResult(DateTime.UtcNow);
        });
        
        var leaveIssuedAt = DateTime.UtcNow;
        
        // An exception is thrown because we close the connection once a player leaves
        Assert.ThrowsAsync<TaskCanceledException>(() => leaderHubConnection.InvokeAsync("LeaveRoom"));
        
        await leaderChangedTcs.Task;
        
        Assert.That(leaveIssuedAt, Is.LessThan(leaderChangedTcs.Task.Result));
    }
    
    [Test]
    public async Task LeaderShouldBeAbleToKickPlayer()
    {
        var (roomCode, leaderHubConnection) = await this.CreateRoomAndConnect("leader");

        const string playerUsername = "player1";
        
        var playerHubConnection = await this.JoinRoomAndConnect(roomCode, playerUsername);
        
        var connectionClosedTcs = new TaskCompletionSource<DateTime>();
        playerHubConnection.Closed += _ =>
        {
            connectionClosedTcs.SetResult(DateTime.UtcNow);
            return Task.CompletedTask;
        };

        var kickIssuedAt = DateTime.UtcNow;
        
        var result = await leaderHubConnection.InvokeAsync<InvocationResultDto<object?>>("KickPlayer", playerUsername);
        
        Assert.That(result.Success, Is.True);
        
        await connectionClosedTcs.Task;
        
        Assert.That(kickIssuedAt, Is.LessThan(connectionClosedTcs.Task.Result));
    }
    
    [Test]
    public async Task LeaderShouldNotBeAbleToKickHimself()
    {
        var (_, leaderHubConnection) = await this.CreateRoomAndConnect("leader");

        var result = await leaderHubConnection.InvokeAsync<InvocationResultDto<object?>>("KickPlayer", "leader");
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
            Assert.That(result.Exception?.Type, Is.EqualTo(ExceptionType.CannotKickLeader));
        });
    }

    [Test]
    public async Task LeaderShouldBeAbleToChangeOptions()
    {
        var (roomCode, leaderHubConnection) = await this.CreateRoomAndConnect("leader");
        
        const string playerUsername = "player1";
        
        var playerHubConnection = await this.JoinRoomAndConnect(roomCode, playerUsername);
        
        var newOptions = new RoomOptions(
            maxPlayersCount: 32,
            possibleRoundTypes: RoomPossibleRoundTypes.Song,
            roundsCount: 3,
            roundDuration: TimeSpan.FromSeconds(20)
        );
        
        var optionsChangedTcs = new TaskCompletionSource<DateTime>();

        playerHubConnection.On<RoomOptions>("RoomOptionsChanged", options => {
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
        await leaderHubConnection.InvokeAsync("SetOptions", newOptions);
        
        await optionsChangedTcs.Task;
        
        Assert.That(changeIssuedAt, Is.LessThan(optionsChangedTcs.Task.Result));
    }
}