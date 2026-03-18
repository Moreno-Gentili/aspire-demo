using Microsoft.AspNetCore.SignalR;

namespace ProgettoAspNetCore;

public class MessageHub : Hub
{
    /*public async Task SendMessage(string message, string timestamp)
    {
        await Clients.All.SendAsync("ReceiveMessage", message, timestamp);
    }*/
}
