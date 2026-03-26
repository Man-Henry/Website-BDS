using Microsoft.EntityFrameworkCore;
using Website_QLPT.Data;
using Website_QLPT.Models;
using Website_QLPT.Services.Notification;

namespace Website_QLPT.Services.Jobs
{
    /// <summary>
    /// Hangfire Job: Gửi email & thông báo nhắc nhở hóa đơn quá hạn cho người thuê.
    /// Lịch chạy: Mỗi ngày 08:00 (cấu hình trong Program.cs via cron expression).
    /// </summary>
    public class InvoiceReminderJob
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<InvoiceReminderJob> _logger;

        public InvoiceReminderJob(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<InvoiceReminderJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("[InvoiceReminderJob] Started at {Time}", DateTime.Now);

            const int reminderDays = 5;
            var thresholdDate = DateTime.Now.AddDays(-reminderDays);

            var overdueInvoices = await _context.Invoices
                .Include(i => i.Contract)
                    .ThenInclude(c => c!.Tenant)
                .Include(i => i.Contract)
                    .ThenInclude(c => c!.Room)
                        .ThenInclude(r => r!.Property)
                .Where(i => i.Status == InvoiceStatus.Unpaid && i.CreatedAt <= thresholdDate)
                .ToListAsync();

            int count = 0;
            foreach (var invoice in overdueInvoices)
            {
                var tenantUserId = invoice.Contract?.Tenant?.IdentityUserId;
                if (string.IsNullOrEmpty(tenantUserId)) continue;

                var title = "⚠️ Nhắc nhở: Hóa đơn chưa thanh toán";
                var message = $"Hóa đơn kỳ {invoice.Month}/{invoice.Year} của phòng {invoice.Contract?.Room?.Name} " +
                              $"(Tổng: {invoice.TotalAmount:N0} VNĐ) đã quá {reminderDays} ngày chưa được thanh toán.";
                var url = $"/Invoices/Details/{invoice.Id}";

                await _notificationService.SendNotificationAsync(tenantUserId, title, message, url);
                count++;
            }

            _logger.LogInformation("[InvoiceReminderJob] Sent {Count} reminders.", count);
        }
    }
}
