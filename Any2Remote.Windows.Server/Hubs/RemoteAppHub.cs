using Microsoft.AspNetCore.SignalR;

namespace Any2Remote.Windows.Server.Hubs;

public class RemoteAppHub : Hub
{
    private readonly ILogger<RemoteAppHub> _logger;

    public RemoteAppHub(ILogger<RemoteAppHub> logger)
    {
        _logger = logger;
    }

    public async override Task OnConnectedAsync()
    {
        _logger.LogInformation("client {connectId} connected to server", Context.ConnectionId);
        await Clients.Caller.SendAsync("ping", Context.ConnectionId);
        await Clients.Caller.SendAsync("RefreshRequired");
        await base.OnConnectedAsync();
    }

    public async Task SendRefreshMessageAsync()
    {
        await Clients.All.SendAsync("RefreshRequired");
    }
}
