namespace Website_QLPT.Services.Email
{
    public class EmailTemplateService : IEmailTemplateService
    {
        /// <summary>
        /// Tạo HTML email template cho hóa đơn tiền nhà.
        /// Thay thế 2 đoạn HTML trùng lặp trong InvoicesController.
        /// </summary>
        public string BuildInvoiceEmail(
            string tenantName,
            string roomName,
            string period,
            decimal amount,
            string actionUrl,
            string ctaText,
            string? statusNote = null)
        {
            var statusHtml = string.IsNullOrEmpty(statusNote)
                ? ""
                : $"<p>Trạng thái hiện tại: <strong>{statusNote}</strong></p>";

            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
                    <h2 style='color: #1a3c5e;'>Hóa Đơn Tiền Nhà</h2>
                    <p>Xin chào <strong>{tenantName}</strong>,</p>
                    <p>Hóa đơn tiền nhà kỳ <strong>{period}</strong> của phòng <strong>{roomName}</strong> đã được hệ thống tạo.</p>
                    <div style='background: #f8fafc; padding: 15px; border-left: 4px solid #f97316; margin: 20px 0;'>
                        <p style='margin: 0; font-size: 16px;'>Tổng số tiền cần thanh toán: <strong style='color: #ef4444; font-size: 18px;'>{amount:N0} VNĐ</strong></p>
                    </div>
                    {statusHtml}
                    <p>Vui lòng click vào nút bên dưới để xem chi tiết:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{actionUrl}' style='background: #f97316; color: white; text-decoration: none; padding: 12px 25px; border-radius: 5px; font-weight: bold; display: inline-block;'>{ctaText}</a>
                    </div>
                    <p style='font-size: 12px; color: #666; border-top: 1px solid #eee; padding-top: 10px;'>
                        Email này được gửi tự động từ Hệ thống Quản lý Phòng Trọ.
                    </p>
                </div>";
        }
    }
}
