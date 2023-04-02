using System.Net;
using System.Net.Http.Json;
using Muuzika.Server.Dtos.Gateway;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using Muuzika.Server.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Enums.Room;
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
}