using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MyFreelance.Web.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task JoinUserGroup(string userId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

    public async Task SendNotification(string userId, string title, string message)
        => await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", title, message);
}
