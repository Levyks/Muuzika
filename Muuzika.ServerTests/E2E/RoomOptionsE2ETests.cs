using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Models;
using Muuzika.ServerTests.E2E.Helpers;
using Muuzika.ServerTests.E2E.Helpers.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        
        var newOptions = new {
            maxPlayersCount = 32,
            possibleRoundTypes = "Both",
            roundCount = 3,
            roundDuration = "00:00:20"
        };
        
        var optionsChangedTcs = new TaskCompletionSource<DateTime>();

        playerConnectedResult.HubConnection.On<RoomOptions>("OptionsChanged", options => {
            Assert.Multiple(() =>
            {
                Assert.That(options.MaxPlayersCount, Is.EqualTo(newOptions.maxPlayersCount));
                Assert.That(options.PossibleRoundTypes.ToString(), Is.EqualTo(newOptions.possibleRoundTypes));
                Assert.That(options.RoundCount, Is.EqualTo(newOptions.roundCount));
                Assert.That(options.RoundDuration.ToString(), Is.EqualTo(newOptions.roundDuration));
            });
            optionsChangedTcs.SetResult(DateTime.UtcNow);
        });
        
        var changeIssuedAt = DateTime.UtcNow;
        await leaderConnectedResult.HubConnection.InvokeAsync("SetOptions", newOptions);
        
        await optionsChangedTcs.Task;
        
        Assert.That(changeIssuedAt, Is.LessThan(optionsChangedTcs.Task.Result));
    }
    
    [Test]
    public async Task RegularPlayerShouldNotBeAbleToChangeOptions()
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");
        
        const string playerUsername = "player1";
        
        var playerConnectedResult = await this.JoinRoomAndConnect(leaderConnectedResult.RoomCode, playerUsername);
        
        var newOptions = new {
            maxPlayersCount = 32,
            possibleRoundTypes = "Both",
            roundCount = 3,
            roundDuration = "00:00:20"
        };
        
        var optionsChangedTcs = new TaskCompletionSource();

        leaderConnectedResult.HubConnection.On<RoomOptions>("OptionsChanged", _ => {
            optionsChangedTcs.SetResult();
        });

        try
        {
            await playerConnectedResult.HubConnection.InvokeAsync("SetOptions", newOptions);
            Assert.Fail("Invocation should have failed");
        }
        catch (HubException ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Failed to invoke 'SetOptions' because user is unauthorized"));
        }
        
        await Task.Delay(500);
        
        Assert.That(optionsChangedTcs.Task.IsCompleted, Is.False);
    }
    
    [Test]
    public async Task NewOptionsShouldBeStoredAndSentInNextSyncAll()
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");
        
        var newOptions = new {
            maxPlayersCount = 32,
            possibleRoundTypes = "Both",
            roundCount = 3,
            roundDuration = "00:00:20"
        };
        await leaderConnectedResult.HubConnection.InvokeAsync("SetOptions", newOptions);
        
        const string playerUsername = "player1";
        
        var playerConnectedResult = await this.JoinRoomAndConnect(leaderConnectedResult.RoomCode, playerUsername);

        var options = playerConnectedResult.ReceivedStateSync.Room.Options;
        Assert.Multiple(() =>
        {
            Assert.That(options.MaxPlayersCount, Is.EqualTo(newOptions.maxPlayersCount));
            Assert.That(options.PossibleRoundTypes.ToString(), Is.EqualTo(newOptions.possibleRoundTypes));
            Assert.That(options.RoundCount, Is.EqualTo(newOptions.roundCount));
            Assert.That(options.RoundDuration.ToString(), Is.EqualTo(newOptions.roundDuration));
        });
    }
    
    [Test]
    [TestCase(0)]
    [TestCase(42)]
    public async Task ShouldNotAllowRoundCountOutOfBounds(int roundCount)
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");
        var expectedMessage = $"The field RoundCount must be between {Configuration["Room:MinRoundsCount"]} and {Configuration["Room:MaxRoundsCount"]}.";

        var newOptions = new {
            maxPlayersCount = 32,
            possibleRoundTypes = "Both",
            roundCount,
            roundDuration = "00:00:20"
        };
        
        try
        {
            await leaderConnectedResult.HubConnection.InvokeAsync("SetOptions", newOptions);
            Assert.Fail("Invocation should have failed");
        }
        catch (HubException ex)
        {
            var exceptionDto = ExceptionMapper.ParseHubException(ex);
            Assert.Multiple(() =>
            {
                Assert.That(exceptionDto.Type, Is.EqualTo(ExceptionType.InvalidArguments));
                Assert.That(exceptionDto.Message, Is.EqualTo("Invalid arguments: RoundCount"));
                
                var data = exceptionDto.Data as JsonElement?;
                
                Assert.That(data, Is.Not.Null);
                Assert.That(data?.GetProperty("memberNames").GetArrayLength(), Is.EqualTo(1));
                Assert.That(data?.GetProperty("memberNames")[0].GetString(), Is.EqualTo("RoundCount"));
                Assert.That(data?.GetProperty("errorMessage").GetString(), Is.EqualTo(expectedMessage));
            });
        }
    }
    
    [Test]
    [TestCase("00:00:01")]
    [TestCase("00:01:00")]
    public async Task ShouldNotAllowRoundDurationOutOfBounds(string roundDuration)
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");
        var expectedMessage = $"The field RoundDuration must be between {Configuration["Room:MinRoundDuration"]} and {Configuration["Room:MaxRoundDuration"]}.";

        var newOptions = new {
            maxPlayersCount = 32,
            possibleRoundTypes = "Both",
            roundCount = 3,
            roundDuration
        };
        
        try
        {
            await leaderConnectedResult.HubConnection.InvokeAsync("SetOptions", newOptions);
            Assert.Fail("Invocation should have failed");
        }
        catch (HubException ex)
        {
            var exceptionDto = ExceptionMapper.ParseHubException(ex);
            Assert.Multiple(() =>
            {
                Assert.That(exceptionDto.Type, Is.EqualTo(ExceptionType.InvalidArguments));
                Assert.That(exceptionDto.Message, Is.EqualTo("Invalid arguments: RoundDuration"));
                
                var data = exceptionDto.Data as JsonElement?;
                
                Assert.That(data, Is.Not.Null);
                Assert.That(data?.GetProperty("memberNames").GetArrayLength(), Is.EqualTo(1));
                Assert.That(data?.GetProperty("memberNames")[0].GetString(), Is.EqualTo("RoundDuration"));
                Assert.That(data?.GetProperty("errorMessage").GetString(), Is.EqualTo(expectedMessage));
            });
        }
    } 
    
    [Test]
    [TestCase(0)]
    [TestCase(42)]
    public async Task ShouldNotAllowMaxPlayersCountOutOfBounds(int maxPlayersCount)
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");
        var expectedMessage = $"The field MaxPlayersCount must be between {Configuration["Room:MinMaxPlayersCount"]} and {Configuration["Room:MaxMaxPlayersCount"]}.";

        var newOptions = new {
            maxPlayersCount,
            possibleRoundTypes = "Both",
            roundCount = 3,
            roundDuration = "00:00:20"
        };
        
        try
        {
            await leaderConnectedResult.HubConnection.InvokeAsync("SetOptions", newOptions);
            Assert.Fail("Invocation should have failed");
        }
        catch (HubException ex)
        {
            var exceptionDto = ExceptionMapper.ParseHubException(ex);
            Assert.Multiple(() =>
            {
                Assert.That(exceptionDto.Type, Is.EqualTo(ExceptionType.InvalidArguments));
                Assert.That(exceptionDto.Message, Is.EqualTo("Invalid arguments: MaxPlayersCount"));
                
                var data = exceptionDto.Data as JsonElement?;
                
                Assert.That(data, Is.Not.Null);
                Assert.That(data?.GetProperty("memberNames").GetArrayLength(), Is.EqualTo(1));
                Assert.That(data?.GetProperty("memberNames")[0].GetString(), Is.EqualTo("MaxPlayersCount"));
                Assert.That(data?.GetProperty("errorMessage").GetString(), Is.EqualTo(expectedMessage));
            });
        }
    }

    [Test]
    public async Task ShouldNotAllowNullArgument()
    {
        var leaderConnectedResult = await this.CreateRoomAndConnect("leader");

        try
        {
            await leaderConnectedResult.HubConnection.InvokeAsync("SetOptions", null);
            Assert.Fail("Invocation should have failed");
        }
        catch (HubException ex)
        {            
            var exceptionDto = ExceptionMapper.ParseHubException(ex);
            Assert.Multiple(() =>
            {
                Assert.That(exceptionDto.Type, Is.EqualTo(ExceptionType.InvalidArguments));
                Assert.That(exceptionDto.Message, Is.EqualTo("Invalid arguments"));
                
                var data = exceptionDto.Data as JsonElement?;
                
                Assert.That(data, Is.Not.Null);
                Assert.That(data?.GetProperty("memberNames").GetArrayLength(), Is.EqualTo(0));
                Assert.That(data?.GetProperty("errorMessage").GetString(), Is.EqualTo("Argument at position 0 must not be null."));
            });
        }
    }
    
}