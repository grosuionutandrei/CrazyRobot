using WebSocketBoilerplate;

namespace Api.WebSocket.ClientDto;

public class ClientIsDisconnectedDto : BaseDto
{
    public string clientId { get; set; }
}