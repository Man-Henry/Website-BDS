using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Website_QLPT.Data;
using Website_QLPT.Models;

namespace Website_QLPT.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer,Identity.Application")]
    public class AppHub : Hub
    {
        private readonly IServiceProvider _serviceProvider;

        public AppHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task OnConnectedAsync()
        {
            var userEmail = Context.User?.Identity?.Name;
            
            // Có thể add user vào group theo Role để tiện broadcast (vd: "Landlords", "Tenants")
            if (Context.User?.IsInRole("Landlord") == true || Context.User?.IsInRole("Admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Landlords");
            }
            if (Context.User?.IsInRole("Tenant") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Tenants");
            }

            // Hoặc add vào group mang tên email của họ để send thẳng cho cá nhân
            if (!string.IsNullOrEmpty(userEmail))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userEmail);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        // Method: Gửi tin nhắn và lưu lịch sử
        public async Task SendMessageToUser(string targetEmail, string message)
        {
            var senderEmail = Context.User?.Identity?.Name ?? "Hệ thống";

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var chatMessage = new ChatMessage
                {
                    SenderEmail = senderEmail,
                    ReceiverEmail = targetEmail,
                    Content = message,
                    SentAt = DateTime.Now
                };

                dbContext.ChatMessages.Add(chatMessage);
                await dbContext.SaveChangesAsync();
            }

            await Clients.Group(targetEmail).SendAsync("ReceiveMessage", senderEmail, message);
        }
    }
}
