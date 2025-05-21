using Api.WebSocket.ClientDto;
using Application.Interfaces.Infrastructure.Websocket;
using Fleck;
using WebSocketBoilerplate;

namespace Api.Websocket.EventHandlers.Robot;

public class ServerDisconnectsClientDto : BaseDto
{
    public bool disconnected { get; set; }
}

public class ClientIsDisconnectedEventHandler(
    IConnectionManager manager
    ) : BaseEventHandler<ClientIsDisconnectedDto>
{
public override async Task Handle(ClientIsDisconnectedDto dto, IWebSocketConnection socket)
{
    
    Console.WriteLine("I am closing the connection");
    
    await manager.RemoveFromDefaultTopics(dto.clientId);

    var response = new ServerDisconnectsClientDto
    {
        disconnected = true,
        requestId = dto.requestId,
        eventType = nameof(ServerDisconnectsClientDto)
    };
    
    socket.SendDto(response);
    socket.Close();
    
    Console.WriteLine("▶️ CLOSED: " + socket.IsAvailable);
}
}
