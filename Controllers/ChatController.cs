using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_QLPT.Data;

namespace Website_QLPT.Controllers
{
    [Authorize(Roles = "Landlord,Admin")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;
            
            // Lấy danh sách Tenant (Khách thuê)
            var tenants = await _context.Tenants
                .Where(t => !t.IsDeleted && t.Email != null)
                .Select(t => new { 
                    t.Id, 
                    t.FullName, 
                    t.Email,
                    HasContracts = t.Contracts.Any()
                })
                .OrderByDescending(t => t.HasContracts)
                .ThenBy(t => t.FullName)
                .ToListAsync();

            ViewBag.Tenants = tenants;
            return View();
        }
    }
}
