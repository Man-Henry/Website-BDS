using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Website_QLPT.Data;
using Website_QLPT.Hubs;
using Website_QLPT.Models;

namespace Website_QLPT.Controllers.Api
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/maintenance")]
    [ApiController]
    [Authorize]
    public class MaintenanceApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<AppHub> _hubContext;

        public MaintenanceApiController(ApplicationDbContext context, IHubContext<AppHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetRequests()
        {
            var userEmail = User.Identity?.Name;
            var isTenant = User.IsInRole("Tenant");

            var query = _context.MaintenanceRequests
                .Include(m => m.Room)
                .Include(m => m.Tenant)
                .AsQueryable();

            if (isTenant)
            {
                // Chỉ lấy báo cáo của người này
                query = query.Where(m => m.Tenant!.Email == userEmail);
            }
            else
            {
                // Landlord/Admin: Lấy báo cáo các phòng thuộc quyền quản lý của họ
                var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(m => m.Room!.Property!.OwnerId == ownerId);
            }

            var requestsRaw = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
            var requests = requestsRaw.Select(m => new
            {
                m.Id,
                m.Title,
                m.Description,
                m.ImageUrl,
                Status = m.Status.ToString(),
                RoomName = m.Room?.Name ?? "N/A",
                TenantName = m.Tenant?.FullName ?? "N/A",
                CreatedAt = m.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                ResolvedAt = m.ResolvedAt?.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            return Ok(requests);
        }

        [HttpPost]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateMaintenanceDto dto)
        {
            var userEmail = User.Identity?.Name;
            
            var tenant = await _context.Tenants
                .Include(t => t.Contracts.Where(c => c.Status == ContractStatus.Active))
                .FirstOrDefaultAsync(t => t.Email == userEmail);

            if (tenant == null || !tenant.Contracts.Any())
            {
                return BadRequest(new { message = "Không tìm thấy hồ sơ hoặc bạn chưa thuê phòng nào." });
            }

            var activeContract = tenant.Contracts.First();

            var request = new MaintenanceRequest
            {
                RoomId = activeContract.RoomId,
                TenantId = tenant.Id,
                Title = dto.Title,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Status = MaintenanceStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            // Bắn notification qua SignalR cho admin/chủ trọ
            await _hubContext.Clients.Group("Landlords").SendAsync("ReceiveNotification", 
                "Hệ thống", $"Phòng {activeContract.Room?.Name} vừa gửi yêu cầu sửa chữa: {request.Title}");

            return Ok(new { message = "Gửi yêu cầu hỗ trợ thành công.", id = request.Id });
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Landlord")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null) return NotFound();

            if (!Enum.TryParse<MaintenanceStatus>(dto.Status, out var newStatus))
            {
                return BadRequest("Trạng thái không hợp lệ. Chỉ chấp nhận: Pending, InProgress, Resolved.");
            }

            request.Status = newStatus;
            
            if (newStatus == MaintenanceStatus.Resolved)
            {
                request.ResolvedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // Bắn notification qua SignalR cho người thuê báo đã cập nhật
            if (request.Tenant?.Email != null)
            {
                await _hubContext.Clients.Group(request.Tenant.Email).SendAsync("ReceiveNotification", 
                    "Hệ thống", $"Yêu cầu '{request.Title}' của bạn đã chuyển sang trạng thái: {newStatus}");
            }

            return Ok(new { message = "Cập nhật trạng thái thành công." });
        }
    }

    public class CreateMaintenanceDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
