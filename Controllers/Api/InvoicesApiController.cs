using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_QLPT.Data;
using Website_QLPT.Models;
using Website_QLPT.Services.Export;

namespace Website_QLPT.Controllers.Api
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/invoices")]
    [ApiController]
    [Authorize]
    public class InvoicesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfExportService _pdfExportService;

        public InvoicesApiController(ApplicationDbContext context, IPdfExportService pdfExportService)
        {
            _context = context;
            _pdfExportService = pdfExportService;
        }

        [HttpGet("{id}/export-pdf")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Contract)
                    .ThenInclude(c => c!.Room)
                        .ThenInclude(r => r!.Property)
                .Include(i => i.Contract)
                    .ThenInclude(c => c!.Tenant)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound(new { message = "Hóa đơn không tồn tại." });
            }

            try
            {
                byte[] pdfBytes = _pdfExportService.GenerateInvoicePdf(invoice);
                return File(pdfBytes, "application/pdf", $"HoaDon_{invoice.Month}_{invoice.Year}_P{invoice.Contract?.Room?.Name}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi tạo file PDF: " + ex.Message });
            }
        }
    }
}
