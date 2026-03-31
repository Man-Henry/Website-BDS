using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_QLPT.Data;
using Website_QLPT.Models;
using Website_QLPT.ViewModels;

namespace Website_QLPT.Controllers
{
    [Authorize(Roles = "Admin,Landlord")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public DashboardController(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> Index()
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(ownerId)) return Forbid();

            ViewData["Title"] = "Tổng Quan & Báo Cáo";

            // ─── Phòng & Hợp đồng ──────────────────────────────────────
            var rooms = await _context.Rooms
                .Include(r => r.Property)
                .Include(r => r.Contracts)
                .Where(r => r.Property!.OwnerId == ownerId)
                .ToListAsync();

            var totalRooms = rooms.Count;
            var occupiedRooms = rooms.Count(r => r.Contracts.Any(c => c.Status == ContractStatus.Active));
            var vacantRooms = totalRooms - occupiedRooms;

            var activeContracts = await _context.Contracts
                .Include(c => c.Room).ThenInclude(r => r!.Property)
                .CountAsync(c => c.Status == ContractStatus.Active && c.Room!.Property!.OwnerId == ownerId);

            // ─── Hóa đơn ──────────────────────────────────────────────
            var invoices = await _context.Invoices
                .Include(i => i.Contract).ThenInclude(c => c!.Room).ThenInclude(r => r!.Property)
                .Include(i => i.Contract).ThenInclude(c => c!.Tenant)
                .Where(i => i.Contract!.Room!.Property!.OwnerId == ownerId)
                .ToListAsync();

            var totalPaid = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount);
            var totalUnpaid = invoices.Where(i => i.Status == InvoiceStatus.Unpaid).Sum(i => i.TotalAmount);

            // ─── Doanh thu 6 tháng gần nhất ───────────────────────────
            var now = DateTime.Now;
            var monthLabels = new List<string>();
            var monthlyRevenue = new List<decimal>();

            for (int i = 5; i >= 0; i--)
            {
                var target = now.AddMonths(-i);
                var label = $"T{target.Month}/{target.Year}";
                var revenue = invoices
                    .Where(inv => inv.Status == InvoiceStatus.Paid
                        && inv.Month == target.Month
                        && inv.Year == target.Year)
                    .Sum(inv => inv.TotalAmount);

                monthLabels.Add(label);
                monthlyRevenue.Add(revenue);
            }

            // ─── Phân phối Phòng theo trạng thái ──────────────────────
            var roomStatusLabels = new List<string> { "Đang thuê", "Còn trống" };
            var roomStatusData = new List<int> { occupiedRooms, vacantRooms };

            // ─── Sự cố & Tickets ────────────────────────────────────────
            var openTickets = await _context.MaintenanceTickets
                .Include(t => t.Contract).ThenInclude(c => c!.Room).ThenInclude(r => r!.Property)
                .Include(t => t.Contract!.Tenant)
                .Where(t => t.Contract!.Room!.Property!.OwnerId == ownerId &&
                            (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress))
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            // ─── Top 5 hóa đơn chưa thu ───────────────────────────────
            var recentUnpaid = invoices
                .Where(i => i.Status == InvoiceStatus.Unpaid)
                .OrderByDescending(i => i.TotalAmount)
                .Take(5)
                .ToList();

            var viewModel = new DashboardViewModel
            {
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                VacantRooms = vacantRooms,
                TotalRevenuePaid = totalPaid,
                TotalRevenueUnpaid = totalUnpaid,
                OpenTickets = openTickets.Count,
                ActiveContracts = activeContracts,
                MonthLabels = monthLabels,
                MonthlyRevenueData = monthlyRevenue,
                RoomStatusLabels = roomStatusLabels,
                RoomStatusData = roomStatusData,
                RecentUnpaidInvoices = recentUnpaid,
                RecentOpenTickets = openTickets
            };

            return View(viewModel);
        }

        // ─── SYSTEM RESET ────────────────────────────────────────────────
        // Chỉ Admin mới có quyền. Xoá toàn bộ dữ liệu nghiệp vụ,
        // giữ lại tài khoản Admin và roles, rồi seed lại data demo.

        [HttpGet]
        public IActionResult SystemReset()
        {
            ViewData["Title"] = "Reset Hệ Thống";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SystemResetConfirm(string confirmation)
        {
            if (confirmation != "RESET")
            {
                TempData["Error"] = "Bạn phải nhập đúng 'RESET' để xác nhận.";
                return RedirectToAction(nameof(SystemReset));
            }

            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Xoá theo thứ tự FK (con trước, cha sau)
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [AuditLogs]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [AppNotifications]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [ChatMessages]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [MaintenanceTickets]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [MaintenanceRequests]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Invoices]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Contracts]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [RoomReviews]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [RoomImages]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Rooms]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [UtilityTiers]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Properties]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [LandlordProfiles]");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [PaymentConfigs]");

            // Xoá tenant users (giữ lại admin)
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Tenants]");
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [AspNetUserRoles] WHERE [UserId] != {0}", adminUserId!);
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [AspNetUsers] WHERE [Id] != {0}", adminUserId!);

            // Re-seed data demo
            await SeedData.Initialize(_serviceProvider);

            TempData["Success"] = "✅ Hệ thống đã được reset thành công. Dữ liệu demo đã được tạo lại.";
            return RedirectToAction(nameof(Index));
        }
    }
}
