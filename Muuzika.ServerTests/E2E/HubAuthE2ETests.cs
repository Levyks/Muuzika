using System.Net;
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
    [TestCase("forty-two")]
    public async Task ShouldNotConnectWithMissingOrInvalidToken(string? token)
    {
        var hubConnection = this.CreateHubConnection(token);

        try
        {
            await hubConnection.StartAsync();
            Assert.Fail("Connection should not be established");
        }
        catch (HttpRequestException ex)
        {
            Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        Assert.That(hubConnection.State, Is.EqualTo(HubConnectionState.Disconnected));
    }
}