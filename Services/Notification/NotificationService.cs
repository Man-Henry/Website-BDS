using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Website_QLPT.Data;
using Website_QLPT.Hubs;
using Website_QLPT.Models;

namespace Website_QLPT.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IServiceProvider _serviceProvider;

        public NotificationService(IHubContext<NotificationHub> hubContext, IServiceProvider serviceProvider)
        {
            _hubContext = hubContext;
            _serviceProvider = serviceProvider;
        }

        public async Task SendNotificationAsync(string userId, string title, string message, string? url = null)
        {
            // Save to DB
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var notification = new AppNotification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Url = url,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                context.AppNotifications.Add(notification);
                await context.SaveChangesAsync();

                var unreadCount = await context.AppNotifications.CountAsync(n => n.UserId == userId && !n.IsRead);

                // Send via SignalR
                await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    url = notification.Url,
                    createdAt = notification.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    unreadCount = unreadCount
                });
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.AppNotifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            }
        }
    }
}
