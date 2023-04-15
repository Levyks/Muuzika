using Microsoft.AspNetCore.SignalR.Client;
using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Dtos.Hub.Responses;

namespace Muuzika.ServerTests.E2E.Helpers;

public record RoomConnectedResultDto(
    string RoomCode,
    string Token,
    HubConnection HubConnection,
    StateSyncDto ReceivedStateSync
    );