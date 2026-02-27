using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MyApp.Api.Infrastructure.Hubs;

[Authorize]
public class AppHub : Hub
{
    private readonly ILogger<AppHub> _logger;

    public AppHub(ILogger<AppHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        if (Context.UserIdentifier is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{Context.UserIdentifier}");

        _logger.LogDebug("Client connected: {ConnectionId}, User: {UserId}",
            Context.ConnectionId, Context.UserIdentifier);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.UserIdentifier is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{Context.UserIdentifier}");

        _logger.LogDebug("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Join a named group (e.g. a board, page, or workspace).</summary>
    public Task JoinGroupAsync(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            throw new HubException("Group name cannot be empty.");

        return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>Leave a named group.</summary>
    public Task LeaveGroupAsync(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            throw new HubException("Group name cannot be empty.");

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
