using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Website_QLPT.Data;
using Website_QLPT.Hubs;
using Website_QLPT.Models;

namespace Website_QLPT.Services.Payment
{
    /// <summary>
    /// Service gộp logic thanh toán hóa đơn — thay thế 4+ đoạn code lặp
    /// trong PaymentController và InvoicesController.
    /// </summary>
    public class InvoicePaymentService : IInvoicePaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _notifHub;

        public InvoicePaymentService(ApplicationDbContext context, IHubContext<NotificationHub> notifHub)
        {
            _context = context;
            _notifHub = notifHub;
        }

        public async Task<bool> MarkAsPaidAsync(Invoice invoice, string paymentMethod, string transactionId, string? auditActionType = null)
        {
            if (invoice.Status == InvoiceStatus.Paid)
                return false; // Đã thanh toán rồi

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.Now;
            invoice.PaymentMethod = paymentMethod;
            invoice.TransactionId = transactionId;

            var ownerId = invoice.Contract?.Room?.Property?.OwnerId ?? "";

            _context.AuditLogs.Add(new AuditLog
            {
                OwnerId = ownerId,
                ActionType = auditActionType ?? "PaymentReceived",
                EntityName = "Invoice",
                EntityId = invoice.Id,
                Details = $"Hóa đơn {invoice.Id} đã được thanh toán qua {paymentMethod} - {invoice.TotalAmount:N0} VNĐ (Kỳ T{invoice.Month}/{invoice.Year})"
            });

            await _context.SaveChangesAsync();

            // Push notification cho chủ trọ
            if (!string.IsNullOrEmpty(ownerId))
            {
                var notif = new AppNotification
                {
                    UserId = ownerId,
                    Title = "✅ Thanh Toán Thành Công",
                    Message = $"Khách phòng {invoice.Contract?.Room?.Name} đã thanh toán {invoice.TotalAmount:N0} VNĐ (Kỳ T{invoice.Month}/{invoice.Year}) qua {paymentMethod}.",
                    Url = $"/Invoices/Details/{invoice.Id}",
                    CreatedAt = DateTime.Now
                };

                _context.AppNotifications.Add(notif);
                await _context.SaveChangesAsync();

                var unreadCount = await _context.AppNotifications.CountAsync(n => n.UserId == ownerId && !n.IsRead);
                await _notifHub.Clients.Group(ownerId).SendAsync("ReceiveNotification", new
                {
                    id = notif.Id,
                    title = notif.Title,
                    message = notif.Message,
                    url = notif.Url,
                    createdAt = notif.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    unreadCount
                });
            }

            return true;
        }
    }
}
