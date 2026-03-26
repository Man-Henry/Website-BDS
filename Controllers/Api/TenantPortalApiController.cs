using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_QLPT.Data;
using Website_QLPT.Models;
using Website_QLPT.Services.Payment;
using Website_QLPT.Services.Security;

namespace Website_QLPT.Controllers.Api
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/tenant-portal")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer,Identity.Application", Roles = "Tenant")]
    public class TenantPortalApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentServiceResolver _paymentResolver;
        private readonly IEncryptionService _encryptionService;
        private readonly IVnPayService _vnPayService;

        public TenantPortalApiController(ApplicationDbContext context, IPaymentServiceResolver paymentResolver, IEncryptionService encryptionService, IVnPayService vnPayService)
        {
            _context = context;
            _paymentResolver = paymentResolver;
            _encryptionService = encryptionService;
            _vnPayService = vnPayService;
        }

        [HttpGet("my-room")]
        public async Task<IActionResult> GetMyRoom()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            // Lấy tenant map với user email
            var tenant = await _context.Tenants
                .Include(t => t.Contracts.Where(c => c.Status == ContractStatus.Active))
                    .ThenInclude(c => c.Room)
                        .ThenInclude(r => r!.Property)
                .FirstOrDefaultAsync(t => t.Email == userEmail && !t.IsDeleted);

            if (tenant == null)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ người thuê." });
            }

            var activeContract = tenant.Contracts.FirstOrDefault();
            if (activeContract == null)
            {
                return Ok(new { message = "Bạn hiện không có phòng nào đang thuê." });
            }

            return Ok(new
            {
                TenantName = tenant.FullName,
                RoomName = activeContract.Room?.Name,
                PropertyName = activeContract.Room?.Property?.Name,
                RentPrice = activeContract.Room?.Price,
                ContractStartDate = activeContract.StartDate.ToString("dd/MM/yyyy"),
                ContractEndDate = activeContract.EndDate?.ToString("dd/MM/yyyy") ?? "Không thời hạn"
            });
        }

        [HttpGet("my-invoices")]
        public async Task<IActionResult> GetMyInvoices()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var tenant = await _context.Tenants
                .Include(t => t.Contracts)
                .FirstOrDefaultAsync(t => t.Email == userEmail && !t.IsDeleted);

            if (tenant == null) return NotFound();

            var contractIds = tenant.Contracts.Select(c => c.Id).ToList();

            var invoicesRaw = await _context.Invoices
                .Include(i => i.Contract)
                    .ThenInclude(c => c!.Room)
                .Where(i => contractIds.Contains(i.ContractId) && !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var invoices = invoicesRaw.Select(i => new
                {
                    i.Id,
                    Month = $"{i.Month}/{i.Year}",
                    TotalAmount = i.TotalAmount,
                    Status = i.Status.ToString(),
                    RoomName = i.Contract?.Room?.Name ?? "N/A",
                    CreatedAt = i.CreatedAt.ToString("dd/MM/yyyy")
                })
                .ToList();

            return Ok(invoices);
        }

        [HttpPost("pay-invoice/{invoiceId}")]
        public async Task<IActionResult> PayInvoice(int invoiceId, [FromQuery] PaymentProvider provider = PaymentProvider.VNPay)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Email == userEmail && !t.IsDeleted);
            if (tenant == null) return NotFound(new { message = "Không tìm thấy hồ sơ người thuê." });

            var invoice = await _context.Invoices
                .Include(i => i.Contract)
                    .ThenInclude(c => c!.Room)
                        .ThenInclude(r => r!.Property)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.Contract!.TenantId == tenant.Id && !i.IsDeleted);

            if (invoice == null || invoice.Status == InvoiceStatus.Paid)
            {
                return BadRequest(new { message = "Hóa đơn không hợp lệ hoặc đã được thanh toán." });
            }

            if (provider == PaymentProvider.VNPay)
            {
                var returnUrl = Url.Action("VnPayReturn", "Payment", null, Request.Scheme) ?? "https://localhost:5001/Payment/VnPayReturn";
                var url = _vnPayService.CreatePaymentUrl(invoice.Id, invoice.TotalAmount, $"Thanh toan tien nha ky {invoice.Month}/{invoice.Year}", returnUrl);
                return Ok(new { payUrl = url });
            }

            var ownerId = invoice.Contract!.Room!.Property!.OwnerId;
            var config = await _context.PaymentConfigs
                .FirstOrDefaultAsync(c => c.OwnerId == ownerId && c.Provider == provider && c.IsActive);

            if (config == null)
            {
                return BadRequest(new { message = "Chủ nhà chưa cấu hình cổng thanh toán này." });
            }

            var decryptedConfig = new PaymentConfig
            {
                Id = config.Id,
                OwnerId = config.OwnerId,
                Provider = config.Provider,
                IsActive = config.IsActive,
                ConfigData = _encryptionService.Decrypt(config.ConfigData)
            };

            var paymentService = _paymentResolver.GetProvider(provider);
            var payUrl = paymentService.CreatePaymentUrl(invoice, decryptedConfig, HttpContext);

            return Ok(new { payUrl = payUrl });
        }

        [HttpGet("maintenance")]
        public async Task<IActionResult> GetMaintenanceTickets()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var tenant = await _context.Tenants.Include(t => t.Contracts).FirstOrDefaultAsync(t => t.Email == userEmail && !t.IsDeleted);
            if (tenant == null) return NotFound(new { message = "Không tìm thấy hồ sơ người thuê." });

            var contractIds = tenant.Contracts.Select(c => c.Id).ToList();
            var tickets = await _context.MaintenanceTickets
                .Where(t => contractIds.Contains(t.ContractId))
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new {
                    t.Id,
                    t.Title,
                    t.Description,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    CreatedAt = t.CreatedAt.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpPost("maintenance")]
        public async Task<IActionResult> CreateMaintenanceTicket([FromBody] CreateMaintenanceTicketRequestDto request)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var tenant = await _context.Tenants.Include(t => t.Contracts).FirstOrDefaultAsync(t => t.Email == userEmail && !t.IsDeleted);
            if (tenant == null) return NotFound(new { message = "Không tìm thấy hồ sơ người thuê." });

            if (!tenant.Contracts.Any(c => c.Id == request.ContractId))
            {
                return BadRequest(new { message = "Hợp đồng không hợp lệ hoặc không thuộc quyền sở hữu của bạn." });
            }

            var ticket = new MaintenanceTicket
            {
                ContractId = request.ContractId,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                Status = TicketStatus.Open,
                CreatedAt = DateTime.Now
            };

            _context.MaintenanceTickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMaintenanceTickets), new { id = ticket.Id }, new {
                ticket.Id,
                ticket.Title,
                ticket.Description,
                Status = ticket.Status.ToString(),
                Priority = ticket.Priority.ToString(),
                CreatedAt = ticket.CreatedAt.ToString("dd/MM/yyyy")
            });
        }
    }

    public class CreateMaintenanceTicketRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public int ContractId { get; set; }
    }
}
