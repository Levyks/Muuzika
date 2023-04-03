using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Muuzika.ServerTests.E2E.Helpers;
using Muuzika.ServerTests.E2E.Helpers.Extensions;
using Muuzika.Server.Enums.Misc;

namespace Muuzika.ServerTests.E2E;

public class HubAuthE2ETests: BaseE2ETest
{
    [Test]
    [TestCase(null)]
    [TestCase("")]
    public async Task ShouldNotConnectWithMissingToken(string? token)
    {
        var hubConnection = this.CreateHubConnection(token);

        var closedTcs = new TaskCompletionSource();
        hubConnection.Closed += ex =>
        {
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex, Is.TypeOf<HubException>());
            if (ex is not HubException hubException) throw new Exception("We should not be here");
            
            var baseException = this.ParseHubException(hubException);
            Assert.That(baseException.Type, Is.EqualTo(ExceptionType.NoTokenProvided));
            
            closedTcs.SetResult();
            return Task.CompletedTask;
        };
        
        await hubConnection.StartAsync();

        await closedTcs.Task;
        
        Assert.That(hubConnection.State, Is.EqualTo(HubConnectionState.Disconnected));
    }
    
    [Test]
    [TestCase("forty-two")]
    public async Task ShouldNotConnectWithInvalidToken(string token)
    {
        var hubConnection = this.CreateHubConnection(token);

        var closedTcs = new TaskCompletionSource();
        hubConnection.Closed += ex =>
        {
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex, Is.TypeOf<HubException>());
            if (ex is not HubException hubException) throw new Exception("We should not be here");
            
            var baseException = this.ParseHubException(hubException);
            Assert.That(baseException.Type, Is.EqualTo(ExceptionType.InvalidToken));
            
            closedTcs.SetResult();
            return Task.CompletedTask;
        };
        
        await hubConnection.StartAsync();

        await closedTcs.Task;
        
        Assert.That(hubConnection.State, Is.EqualTo(HubConnectionState.Disconnected));
    }
}