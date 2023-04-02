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
        
        var tcs = new TaskCompletionSource<bool>();
        
        hubConnection.On<string>("PlayerJoined", username =>
        {
            Assert.That(username, Is.EqualTo(playerUsername));
            tcs.SetResult(true);
        });
        
        await this.JoinRoomAndConnect(roomCode, playerUsername);
        
        Assert.That(await tcs.Task, Is.True);
    }
}