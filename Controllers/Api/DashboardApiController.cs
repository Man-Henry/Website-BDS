using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_QLPT.Data;
using Website_QLPT.Models;

namespace Website_QLPT.Controllers.Api
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/dashboard")]
    [ApiController]
    [Authorize(Roles = "Admin,Landlord")]
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Optimization: Get only the properties belonging to the user
            var propertyIds = await _context.Properties
                .Where(p => p.OwnerId == userId)
                .Select(p => p.Id)
                .ToListAsync();

            if (!propertyIds.Any())
            {
                return Ok(new
                {
                    TotalRooms = 0,
                    RentedRooms = 0,
                    EmptyRooms = 0,
                    RevenueThisMonth = 0,
                    TotalDebt = 0,
                    ExpiringContractsCount = 0,
                    ExpiringContracts = new List<object>()
                });
            }

            // 1. Tổng quan phòng
            var rooms = await _context.Rooms
                .Where(r => propertyIds.Contains(r.PropertyId))
                .Select(r => r.Status)
                .ToListAsync();

            int totalRooms = rooms.Count;
            int rentedRooms = rooms.Count(r => r == RoomStatus.Rented);
            int emptyRooms = rooms.Count(r => r == RoomStatus.Available || r == RoomStatus.Maintenance);

            // 2. Doanh thu trong tháng hiện tại
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var invoicesThisMonth = await _context.Invoices
                .Where(i => i.Month == currentMonth && i.Year == currentYear && propertyIds.Contains(i.Contract!.Room!.PropertyId))
                .Select(i => new { i.Status, TotalAmount = i.RoomFee + i.ElectricityFee + i.WaterFee + i.OtherFee })
                .ToListAsync();

            decimal revenueThisMonth = invoicesThisMonth
                .Where(i => i.Status == InvoiceStatus.Paid)
                .Sum(i => i.TotalAmount);

            // 3. Tổng nợ tồn đọng (tất cả các tháng)
            var allUnpaidInvoices = await _context.Invoices
                .Where(i => i.Status == InvoiceStatus.Unpaid && propertyIds.Contains(i.Contract!.Room!.PropertyId))
                .Select(i => new { TotalAmount = i.RoomFee + i.ElectricityFee + i.WaterFee + i.OtherFee })
                .ToListAsync();

            decimal totalDebt = allUnpaidInvoices.Sum(i => i.TotalAmount);

            // 4. Hợp đồng sắp hết hạn (trong 30 ngày)
            var thirtyDaysFromNow = DateTime.Now.AddDays(30);
            var expiringContractsRaw = await _context.Contracts
                .Include(c => c.Room)
                .Include(c => c.Tenant)
                .Where(c => propertyIds.Contains(c.Room!.PropertyId) && c.Status == ContractStatus.Active && c.EndDate <= thirtyDaysFromNow)
                .OrderBy(c => c.EndDate)
                .Take(5)
                .ToListAsync();

            var expiringContracts = expiringContractsRaw
                .Select(c => new
                {
                    c.Id,
                    RoomName = c.Room?.Name,
                    TenantName = c.Tenant?.FullName,
                    EndDate = c.EndDate.HasValue ? c.EndDate.Value.ToString("dd/MM/yyyy") : "N/A",
                    DaysLeft = c.EndDate.HasValue ? (int)(c.EndDate.Value.Date - DateTime.Now.Date).TotalDays : 0
                })
                .ToList();

            return Ok(new
            {
                TotalRooms = totalRooms,
                RentedRooms = rentedRooms,
                EmptyRooms = emptyRooms,
                RevenueThisMonth = revenueThisMonth,
                TotalDebt = totalDebt,
                ExpiringContractsCount = expiringContracts.Count,
                ExpiringContracts = expiringContracts
            });
        }
    }
}
