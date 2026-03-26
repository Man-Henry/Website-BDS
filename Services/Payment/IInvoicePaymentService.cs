using Website_QLPT.Models;

namespace Website_QLPT.Services.Payment
{
    public interface IInvoicePaymentService
    {
        /// <summary>
        /// Đánh dấu hóa đơn đã thanh toán, tạo AuditLog, lưu DB và push notification.
        /// Gộp logic lặp 4+ lần trong PaymentController và InvoicesController.
        /// </summary>
        Task<bool> MarkAsPaidAsync(Invoice invoice, string paymentMethod, string transactionId, string? auditActionType = null);
    }
}
