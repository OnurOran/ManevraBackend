using Microsoft.AspNetCore.SignalR;

namespace MyApp.Api.Infrastructure.Hubs;

public class NotificationService : INotificationService
{
    private readonly IHubContext<AppHub> _hub;

    public NotificationService(IHubContext<AppHub> hub)
    {
        _hub = hub;
    }

    public Task SendToUserAsync(string userId, string eventName, object payload, CancellationToken ct = default) =>
        _hub.Clients.User(userId).SendAsync(eventName, payload, ct);

    public Task SendToGroupAsync(string group, string eventName, object payload, CancellationToken ct = default) =>
        _hub.Clients.Group(group).SendAsync(eventName, payload, ct);

    public Task BroadcastAsync(string eventName, object payload, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync(eventName, payload, ct);
}
