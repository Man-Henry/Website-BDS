using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_QLPT.Data;
using Website_QLPT.Models;
using Website_QLPT.Services.Security;
using System.Text.Json;

namespace Website_QLPT.Controllers
{
    [Authorize(Roles = "Admin,Landlord")]
    public class PaymentConfigsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public PaymentConfigsController(ApplicationDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        public async Task<IActionResult> Index()
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(ownerId)) return Forbid();

            var configs = await _context.PaymentConfigs
                .Where(c => c.OwnerId == ownerId)
                .ToListAsync();

            return View(configs);
        }

        public IActionResult Create()
        {
            return View(new PaymentConfig { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Provider,IsActive")] PaymentConfig config, string unencryptedConfigData)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(ownerId)) return Forbid();

            config.OwnerId = ownerId;
            
            // Check if already exists for this provider
            var exists = await _context.PaymentConfigs.AnyAsync(c => c.OwnerId == ownerId && c.Provider == config.Provider);
            if (exists)
            {
                ModelState.AddModelError("Provider", "Bạn đã cấu hình cổng thanh toán này rồi. Vui lòng chỉnh sửa thay vì tạo mới.");
            }

            if (string.IsNullOrWhiteSpace(unencryptedConfigData))
            {
                ModelState.AddModelError("ConfigData", "Vui lòng nhập JSON cấu hình.");
            }
            else
            {
                try
                {
                    // Validate JSON format
                    JsonDocument.Parse(unencryptedConfigData);
                    config.ConfigData = _encryptionService.Encrypt(unencryptedConfigData);
                }
                catch
                {
                    ModelState.AddModelError("ConfigData", "Định dạng cấu hình không phải là chuỗi JSON hợp lệ.");
                }
            }

            ModelState.Remove("OwnerId");
            ModelState.Remove("ConfigData");

            if (ModelState.IsValid)
            {
                _context.Add(config);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã thêm cấu hình thanh toán thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(config);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var config = await _context.PaymentConfigs.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
            
            if (config == null) return NotFound();

            ViewBag.DecryptedData = _encryptionService.Decrypt(config.ConfigData);
            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Provider,IsActive")] PaymentConfig config, string unencryptedConfigData)
        {
            if (id != config.Id) return NotFound();

            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dbConfig = await _context.PaymentConfigs.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
            if (dbConfig == null) return NotFound();

            if (string.IsNullOrWhiteSpace(unencryptedConfigData))
            {
                ModelState.AddModelError("ConfigData", "Vui lòng nhập JSON cấu hình.");
            }
            else
            {
                try
                {
                    JsonDocument.Parse(unencryptedConfigData);
                    dbConfig.ConfigData = _encryptionService.Encrypt(unencryptedConfigData);
                }
                catch
                {
                    ModelState.AddModelError("ConfigData", "Định dạng cấu hình không phải là chuỗi JSON hợp lệ.");
                }
            }

            dbConfig.IsActive = config.IsActive;

            ModelState.Remove("OwnerId");
            ModelState.Remove("ConfigData");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dbConfig);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật cấu hình thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentConfigExists(dbConfig.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.DecryptedData = unencryptedConfigData;
            return View(dbConfig);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var config = await _context.PaymentConfigs.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
            if (config != null)
            {
                _context.PaymentConfigs.Remove(config);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa cấu hình thanh toán.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentConfigExists(int id)
        {
            return _context.PaymentConfigs.Any(e => e.Id == id);
        }
    }
}
