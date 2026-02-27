namespace MyApp.Api.Infrastructure.Hubs;

/// <summary>Sends real-time events to connected clients via SignalR.</summary>
public interface INotificationService
{
    Task SendToUserAsync(string userId, string eventName, object payload, CancellationToken ct = default);
    Task SendToGroupAsync(string group, string eventName, object payload, CancellationToken ct = default);
    Task BroadcastAsync(string eventName, object payload, CancellationToken ct = default);
}
