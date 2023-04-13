using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Enums.Room;

namespace Muuzika.ServerTests.E2E.Helpers.Extensions;

public static class RoomTestsExtensions
{
    public static HubConnection CreateHubConnection(this BaseE2ETest test, string? token)
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(test.Factory.Server.BaseAddress + "hub",  options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => test.Factory.Server.CreateHandler();
                options.Transports = HttpTransportType.ServerSentEvents;
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .Build();
        
        return hubConnection;
    }
    
    public static async Task<RoomCreatedOrJoinedDto> CreateRoom(this BaseE2ETest test, string username)
    {
        const string captchaToken = "foo";

        var body = new CreateOrJoinRoomDto(username, captchaToken);
        var response = await test.Client.PostAsJsonAsync("/room", body);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var contentString = await response.Content.ReadAsStringAsync();
        var roomCreatedOrJoinedDto = JsonSerializer.Deserialize<RoomCreatedOrJoinedDto>(contentString, test.JsonSerializerOptions);
        
        Assert.That(roomCreatedOrJoinedDto, Is.Not.Null);
        if (roomCreatedOrJoinedDto == null) throw new Exception("We should not be here");
        
        Assert.Multiple(() =>
        {
            Assert.That(roomCreatedOrJoinedDto.Token, Is.Not.Null);
            Assert.That(roomCreatedOrJoinedDto.Username, Is.EqualTo(username));
            Assert.That(roomCreatedOrJoinedDto.RoomCode, Is.Not.Null);
        });

        return roomCreatedOrJoinedDto;
    }
    
    public static async Task<RoomConnectedResultDto> ConnectToRoom(this BaseE2ETest test, RoomCreatedOrJoinedDto roomCreatedOrJoinedDto)
    {
        var hubConnection = test.CreateHubConnection(roomCreatedOrJoinedDto.Token);

        await hubConnection.StartAsync();
        
        var stateSync = await hubConnection.InvokeAsync<StateSyncDto>("SyncAll");
        
        var room = stateSync.Room;
        var player = stateSync.Player;
            
        Assert.Multiple(() =>
        {
            Assert.That(room.Code, Is.EqualTo(roomCreatedOrJoinedDto.RoomCode));;

            Assert.That(player.Username, Is.EqualTo(roomCreatedOrJoinedDto.Username));
            Assert.That(player.Score, Is.EqualTo(0));
            Assert.That(player.IsConnected, Is.True);
        });

        return new RoomConnectedResultDto(
            RoomCode: roomCreatedOrJoinedDto.RoomCode,
            Token: roomCreatedOrJoinedDto.Token,
            HubConnection: hubConnection,
            ReceivedStateSync: stateSync
        );
    }

    public static async Task<RoomConnectedResultDto> CreateRoomAndConnect(this BaseE2ETest test, string username)
    {
        var createdDto = await test.CreateRoom(username);
        
        var connectedResult = await test.ConnectToRoom(createdDto);
        
        var room = connectedResult.ReceivedStateSync.Room;
        
        Assert.Multiple(() =>
        {
            Assert.That(room.Players.Count(), Is.EqualTo(1));
            Assert.That(room.Players.First().Username, Is.EqualTo(username));
            Assert.That(room.LeaderUsername, Is.EqualTo(username));
            Assert.That(room.Status, Is.EqualTo(RoomStatus.InLobby));
            
            Assert.That(room.Options.RoundDuration, Is.EqualTo(test.ConfigProviderMock.Object.RoomDefaultRoundDuration));
            Assert.That(room.Options.RoundsCount, Is.EqualTo(test.ConfigProviderMock.Object.RoomDefaultRoundsCount));
            Assert.That(room.Options.MaxPlayersCount, Is.EqualTo(test.ConfigProviderMock.Object.RoomDefaultMaxPlayersCount));
            Assert.That(room.Options.PossibleRoundTypes, Is.EqualTo(test.ConfigProviderMock.Object.RoomDefaultPossibleRoundTypes));
        });
        
        return connectedResult;
    }
    
    public static async Task<RoomCreatedOrJoinedDto> JoinRoom(this BaseE2ETest test, string roomCode, string username)
    {
        const string captchaToken = "foo";

        var body = new CreateOrJoinRoomDto(username, captchaToken);
        var response = await test.Client.PostAsJsonAsync($"/room/{roomCode}", body);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var contentString = await response.Content.ReadAsStringAsync();
        var roomCreatedOrJoinedDto = JsonSerializer.Deserialize<RoomCreatedOrJoinedDto>(contentString, test.JsonSerializerOptions);
        
        Assert.That(roomCreatedOrJoinedDto, Is.Not.Null);
        if (roomCreatedOrJoinedDto == null) throw new Exception("We should not be here");
        
        Assert.Multiple(() =>
        {
            Assert.That(roomCreatedOrJoinedDto.Token, Is.Not.Null);
            Assert.That(roomCreatedOrJoinedDto.Username, Is.EqualTo(username));
            Assert.That(roomCreatedOrJoinedDto.RoomCode, Is.EqualTo(roomCode));
        });

        return roomCreatedOrJoinedDto;
    }

    public static async Task<RoomConnectedResultDto> JoinRoomAndConnect(this BaseE2ETest test, string roomCode, string username)
    {
        var joinedDto = await test.JoinRoom(roomCode, username);
        
        var connectedResult = await test.ConnectToRoom(joinedDto);
        
        var room = connectedResult.ReceivedStateSync.Room;
            
        Assert.Multiple(() =>
        {
            Assert.That(room.Players, Has.Exactly(1).Property("Username").EqualTo(username));
        });

        return connectedResult;
    }
}