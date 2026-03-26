using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_QLPT.Data;
using Website_QLPT.Models;

namespace Website_QLPT.Controllers
{
    [Authorize(Roles = "Tenant")]
    public class RoomReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RoomReviews/Create/5
        public async Task<IActionResult> Create(int roomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Find tenant associated with this user
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.IdentityUserId == userId);
            if (tenant == null)
            {
                TempData["Error"] = "Hồ sơ khách thuê không tồn tại.";
                return RedirectToAction("Index", "TenantDashboard");
            }

            // Verify they have rented this room
            var hasRented = await _context.Contracts
                .AnyAsync(c => c.TenantId == tenant.Id && c.RoomId == roomId);

            if (!hasRented)
            {
                TempData["Error"] = "Bạn chỉ có thể đánh giá những phòng mà bạn đã hoặc đang thuê.";
                return RedirectToAction("Index", "TenantDashboard");
            }

            // Check if already reviewed
            var existingReview = await _context.RoomReviews
                .FirstOrDefaultAsync(r => r.TenantId == tenant.Id && r.RoomId == roomId);

            if (existingReview != null)
            {
                TempData["Error"] = "Bạn đã đánh giá phòng này rồi.";
                return RedirectToAction("Index", "TenantDashboard");
            }

            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return NotFound();

            ViewBag.RoomName = room.Name;
            return View(new RoomReview { RoomId = roomId, TenantId = tenant.Id });
        }

        // POST: RoomReviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,TenantId,Rating,Comment")] RoomReview roomReview)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.IdentityUserId == userId);
            
            if (tenant == null || tenant.Id != roomReview.TenantId)
            {
                return Unauthorized();
            }

            var hasRented = await _context.Contracts
                .AnyAsync(c => c.TenantId == tenant.Id && c.RoomId == roomReview.RoomId);

            if (!hasRented)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                roomReview.CreatedAt = DateTime.Now;
                roomReview.IsApproved = true; // Auto approve
                _context.Add(roomReview);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cảm ơn bạn đã gửi đánh giá!";
                return RedirectToAction("Index", "TenantDashboard");
            }

            var room = await _context.Rooms.FindAsync(roomReview.RoomId);
            ViewBag.RoomName = room?.Name;
            return View(roomReview);
        }
    }
}
