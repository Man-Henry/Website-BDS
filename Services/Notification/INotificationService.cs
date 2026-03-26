namespace Website_QLPT.Services.Notification
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, string? url = null);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
