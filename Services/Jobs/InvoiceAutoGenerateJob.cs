using Microsoft.EntityFrameworkCore;
using Website_QLPT.Data;
using Website_QLPT.Models;
using Website_QLPT.Services.Notification;

namespace Website_QLPT.Services.Jobs
{
    /// <summary>
    /// Hangfire Job: Tự động tạo hóa đơn cho tất cả hợp đồng đang active.
    /// Lịch chạy: 00:01 ngày mùng 1 hàng tháng.
    /// </summary>
    public class InvoiceAutoGenerateJob
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<InvoiceAutoGenerateJob> _logger;

        public InvoiceAutoGenerateJob(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<InvoiceAutoGenerateJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var now = DateTime.Now;
            int month = now.Month;
            int year  = now.Year;

            _logger.LogInformation("[InvoiceAutoGenerateJob] Generating invoices for {Month}/{Year}", month, year);

            var activeContracts = await _context.Contracts
                .Include(c => c.Room)
                    .ThenInclude(r => r!.Property)
                        .ThenInclude(p => p!.UtilityTiers)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            int created = 0;
            int skipped = 0;

            foreach (var contract in activeContracts)
            {
                // Bỏ qua nếu đã tạo hóa đơn kỳ này rồi (idempotent)
                bool exists = await _context.Invoices.AnyAsync(
                    i => i.ContractId == contract.Id && i.Month == month && i.Year == year);

                if (exists) { skipped++; continue; }

                var invoice = new Invoice
                {
                    ContractId      = contract.Id,
                    Month           = month,
                    Year            = year,
                    RoomFee         = contract.Room?.Price ?? 0,
                    ElectricityPrice = 3500,
                    WaterPrice      = 15000,
                    Status          = InvoiceStatus.Unpaid,
                    CreatedAt       = DateTime.Now
                };

                _context.Invoices.Add(invoice);
                created++;

                // Thông báo Chủ trọ về hóa đơn mới được tạo
                var ownerId = contract.Room?.Property?.OwnerId;
                if (!string.IsNullOrEmpty(ownerId))
                {
                    await _notificationService.SendNotificationAsync(
                        ownerId,
                        $"Hóa đơn T{month}/{year} đã được tạo",
                        $"Hệ thống đã tự động tạo hóa đơn cho phòng {contract.Room?.Name} (Tiền phòng: {contract.Room?.Price:N0} VNĐ).",
                        "/Invoices");
                }
            }

            if (created > 0) await _context.SaveChangesAsync();

            _logger.LogInformation("[InvoiceAutoGenerateJob] Created {Created} invoices, skipped {Skipped}.", created, skipped);
        }
    }
}
