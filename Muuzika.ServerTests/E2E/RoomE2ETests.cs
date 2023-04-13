using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Dtos.Misc;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Enums.Room;
using Muuzika.Server.Models;
using Muuzika.ServerTests.E2E.Helpers;
using Muuzika.ServerTests.E2E.Helpers.Extensions;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Providers.Interfaces;

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
    public async Task ShouldCreate10000RoomsAndFailToCreateOneMore()
    {
        await Task.WhenAll(Enumerable.Range(0, 10000).Select(_ => this.CreateRoom("leader")));
        
        var body = new CreateOrJoinRoomDto("foo", "bar");
        var response = await Client.PostAsJsonAsync("/room", body);
        
        var contentString = await response.Content.ReadAsStringAsync();
        var baseExceptionDto = JsonSerializer.Deserialize<BaseExceptionDto>(contentString, Factory.Services.GetRequiredService<JsonSerializerOptions>());
        
        Assert.That(baseExceptionDto, Is.Not.Null);
        if (baseExceptionDto == null) throw new Exception("We should not be here");
        
        Assert.That(baseExceptionDto.Type, Is.EqualTo(ExceptionType.OutOfAvailableRoomCodes));
    }

    [Test]
    public async Task ShouldJoinAndConnectAndOtherPlayersShouldBeNotified()
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");

        const string playerUsername = "player1";
        
        var joinedTcs = new TaskCompletionSource<DateTime>();
        leaderConnectedResult.HubConnection.On<string>("PlayerJoined", username =>
        {
            Assert.That(username, Is.EqualTo(playerUsername));
            joinedTcs.SetResult(DateTime.UtcNow);
        });
        
        var isConnectedChangedTcs = new TaskCompletionSource<DateTime>();
        leaderConnectedResult.HubConnection.On<string, bool>("PlayerIsConnectedChanged", (username, isConnected) => {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.EqualTo(playerUsername));
                Assert.That(isConnected, Is.True);
            });
            isConnectedChangedTcs.SetResult(DateTime.UtcNow);
        });
        
        var joinIssuedAt = DateTime.UtcNow;
        await this.JoinRoomAndConnect(leaderConnectedResult.RoomCode, playerUsername);
        
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
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");

        const string playerUsername = "player1";
        
        var playerConnectedResult = await this.JoinRoomAndConnect(leaderConnectedResult.RoomCode, playerUsername);
        
        var isConnectedChangedTcs = new TaskCompletionSource<DateTime>();
        leaderConnectedResult.HubConnection.On<string, bool>("PlayerIsConnectedChanged", (username, isConnected) => {
            Assert.Multiple(() =>
            {
                Assert.That(username, Is.EqualTo(playerUsername));
                Assert.That(isConnected, Is.False);
            });
            isConnectedChangedTcs.SetResult(DateTime.UtcNow);
        });
        
        var disconnectedIssuedAt = DateTime.UtcNow;
        await playerConnectedResult.HubConnection.StopAsync();
        
        await isConnectedChangedTcs.Task;
        
        Assert.That(disconnectedIssuedAt, Is.LessThan(isConnectedChangedTcs.Task.Result));
    }

    [Test]
    public async Task ShouldMakeOtherPlayerLeaderInCaseHeLeaves()
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");

        const string playerUsername = "player1";
        
        var playerConnectedResult = await this.JoinRoomAndConnect(leaderConnectedResult.RoomCode, playerUsername);
        
        var leaderChangedTcs = new TaskCompletionSource<DateTime>();
        playerConnectedResult.HubConnection.On<string>("RoomLeaderChanged", username => {
            Assert.That(username, Is.EqualTo(playerUsername));
            leaderChangedTcs.SetResult(DateTime.UtcNow);
        });
        
        var leaveIssuedAt = DateTime.UtcNow;
        
        // An exception is thrown because we close the connection once a player leaves
        Assert.ThrowsAsync<TaskCanceledException>(() => leaderConnectedResult.HubConnection.InvokeAsync("LeaveRoom"));
        
        await leaderChangedTcs.Task;
        
        Assert.That(leaveIssuedAt, Is.LessThan(leaderChangedTcs.Task.Result));
    }
    
    [Test]
    public async Task LeaderShouldBeAbleToKickPlayer()
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");

        const string playerUsername = "player1";
        
        var playerConnectedResult = await this.JoinRoomAndConnect(leaderConnectedResult.RoomCode, playerUsername);
        
        var connectionClosedTcs = new TaskCompletionSource<DateTime>();
        playerConnectedResult.HubConnection.Closed += _ =>
        {
            connectionClosedTcs.SetResult(DateTime.UtcNow);
            return Task.CompletedTask;
        };

        var kickIssuedAt = DateTime.UtcNow;
        
        await leaderConnectedResult.HubConnection.InvokeAsync("KickPlayer", playerUsername);
        
        await connectionClosedTcs.Task;
        
        Assert.That(kickIssuedAt, Is.LessThan(connectionClosedTcs.Task.Result));
    }
    
    [Test]
    public async Task LeaderShouldNotBeAbleToKickHimself()
    {
        var connectedResult = await this.CreateRoomAndConnect("leader");

        try
        {
            await connectedResult.HubConnection.InvokeAsync<InvocationResultDto<object?>>("KickPlayer", "leader");
            Assert.Fail("Should not succeed");
        }
        catch (HubException ex)
        {
            var exceptionDto = ExceptionMapper.ParseHubException(ex);
            Assert.That(exceptionDto.Type, Is.EqualTo(ExceptionType.CannotKickLeader));
        }
    }
}