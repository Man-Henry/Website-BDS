using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Website_QLPT.Data;
using Website_QLPT.Hubs;
using Website_QLPT.Models;
using Website_QLPT.Services;
using Website_QLPT.Services.Payment;
using Website_QLPT.Services.Security;

namespace Website_QLPT.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentServiceResolver _paymentResolver;
        private readonly IEncryptionService _encryptionService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IVnPayService _vnPayService;
        private readonly IInvoicePaymentService _invoicePaymentService;

        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ApplicationDbContext context, 
            IPaymentServiceResolver paymentResolver,
            IEncryptionService encryptionService,
            ICurrentTenantService currentTenantService,
            IVnPayService vnPayService,
            IInvoicePaymentService invoicePaymentService,
            IConfiguration configuration,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _paymentResolver = paymentResolver;
            _encryptionService = encryptionService;
            _currentTenantService = currentTenantService;
            _vnPayService = vnPayService;
            _invoicePaymentService = invoicePaymentService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int invoiceId, PaymentProvider provider)
        {
            var tenant = await _currentTenantService.GetCurrentTenantAsync(User);
            if (tenant == null)
            {
                TempData["Error"] = "Không tìm thấy hồ sơ người thuê.";
                return RedirectToAction("Index", "TenantDashboard");
            }

            var invoice = await GetInvoiceWithDetailsAsync(i => i.Id == invoiceId && i.Contract!.TenantId == tenant.Id);

            if (invoice == null || invoice.Status == InvoiceStatus.Paid)
            {
                TempData["Error"] = "Hóa đơn không hợp lệ hoặc đã được thanh toán.";
                return RedirectToAction("Details", "Invoices", new { id = invoiceId });
            }

            // Fallback for VNPay without dynamic config
            if (provider == PaymentProvider.VNPay)
            {
                var returnUrl = Url.Action("VnPayReturn", "Payment", null, Request.Scheme) ?? "";
                var url = _vnPayService.CreatePaymentUrl(invoice.Id, invoice.TotalAmount, $"Thanh toan tien nha ky {invoice.Month}/{invoice.Year}", returnUrl);
                return Redirect(url);
            }

            var ownerId = invoice.Contract!.Room!.Property!.OwnerId;
            var config = await _context.PaymentConfigs
                .FirstOrDefaultAsync(c => c.OwnerId == ownerId && c.Provider == provider && c.IsActive);

            if (config == null)
            {
                TempData["Error"] = "Chủ nhà chưa cấu hình hoặc đã tắt cổng thanh toán này.";
                return RedirectToAction("Details", "Invoices", new { id = invoiceId });
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

            return Redirect(payUrl);
        }

        [AllowAnonymous]
        public IActionResult MockCheckout(string provider, string amount, string orderId, string returnUrl)
        {
            ViewBag.Provider = provider;
            ViewBag.Amount = amount;
            ViewBag.OrderId = orderId;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult MockProcessPayment(string orderId, string returnUrl, bool success)
        {
            var separator = returnUrl.Contains("?") ? "&" : "?";
            var finalUrl = $"{returnUrl}{separator}orderId={orderId}&resultCode={(success ? "0" : "1")}";
            return Redirect(finalUrl);
        }

        [AllowAnonymous]
        public async Task<IActionResult> MoMoReturn(string orderId, string resultCode)
        {
            return await ProcessReturn(orderId, resultCode, PaymentProvider.MoMo);
        }

        [AllowAnonymous]
        public async Task<IActionResult> PayOSReturn(string orderId, string resultCode)
        {
            return await ProcessReturn(orderId, resultCode, PaymentProvider.PayOS);
        }

        private async Task<IActionResult> ProcessReturn(string orderId, string resultCode, PaymentProvider provider)
        {
            var invoiceId = ParseInvoiceIdFromOrderId(orderId, "INV_");
            if (invoiceId == null)
            {
                TempData["Error"] = "Giao dịch không hợp lệ.";
                return RedirectToAction("Index", "TenantDashboard");
            }

            var invoice = await GetInvoiceWithDetailsAsync(i => i.Id == invoiceId.Value);
            if (invoice == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Index", "TenantDashboard");
            }

            if (resultCode == "0")
            {
                await _invoicePaymentService.MarkAsPaidAsync(invoice, provider.ToString(), orderId);
                TempData["Success"] = $"Thanh toán hóa đơn thành công qua {provider}!";
            }
            else
            {
                TempData["Error"] = "Thanh toán đã bị hủy hoặc thất bại.";
            }

            return RedirectToAction("Details", "Invoices", new { id = invoiceId.Value });
        }

        [HttpGet]
        public async Task<IActionResult> VnPayReturn()
        {
            var query = HttpContext.Request.Query;
            var vnp_ResponseCode = query["vnp_ResponseCode"].ToString();
            var vnp_TxnRef = query["vnp_TxnRef"].ToString();

            if (!string.IsNullOrEmpty(vnp_TxnRef))
            {
                var invoiceId = ParseInvoiceIdFromOrderId(vnp_TxnRef, splitChar: '_');
                if (invoiceId != null)
                {
                    var invoice = await GetInvoiceWithDetailsAsync(i => i.Id == invoiceId.Value);
                    if (invoice != null)
                    {
                        if (vnp_ResponseCode == "00")
                        {
                            var transactionNo = query["vnp_TransactionNo"].ToString();
                            await _invoicePaymentService.MarkAsPaidAsync(invoice, "VNPAY", transactionNo);
                            TempData["Success"] = "Thanh toán hóa đơn thành công qua VNPAY!";
                        }
                        else
                        {
                            TempData["Error"] = $"Thanh toán thất bại hoặc bị hủy (Mã lỗi: {vnp_ResponseCode}).";
                        }
                        return RedirectToAction("Details", "Invoices", new { id = invoiceId.Value });
                    }
                }
            }

            TempData["Error"] = "Lỗi trong quá trình xử lý giao dịch từ VNPAY.";
            return RedirectToAction("Index", "TenantDashboard");
        }

        // ─────────────────────────────────────────────────────────────────────
        // IPN WEBHOOKS (Server-to-Server callbacks from Payment Gateways)
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        [Route("payment/vnpay-ipn")]
        public async Task<IActionResult> VnPayIPN()
        {
            try
            {
                var query = HttpContext.Request.Query;
                var vnp_SecureHash = query["vnp_SecureHash"].ToString();
                var vnp_ResponseCode = query["vnp_ResponseCode"].ToString();
                var vnp_TxnRef = query["vnp_TxnRef"].ToString();
                var vnp_TransactionNo = query["vnp_TransactionNo"].ToString();

                // Tái tạo raw data để verify HMAC
                var rawParams = query
                    .Where(q => !q.Key.StartsWith("vnp_SecureHash"))
                    .OrderBy(q => q.Key)
                    .Select(q => $"{q.Key}={q.Value}")
                    .ToList();
                var rawData = string.Join("&", rawParams);

                if (!_vnPayService.ValidateSignature(rawData, vnp_SecureHash))
                {
                    _logger.LogWarning("[VNPay IPN] Invalid signature for TxnRef={TxnRef}", vnp_TxnRef);
                    return Ok(new { RspCode = "97", Message = "Invalid signature" });
                }

                var invoiceId = ParseInvoiceIdFromOrderId(vnp_TxnRef, splitChar: '_');
                if (invoiceId == null)
                    return Ok(new { RspCode = "01", Message = "Invalid TxnRef" });

                var invoice = await GetInvoiceWithDetailsAsync(i => i.Id == invoiceId.Value);
                if (invoice == null) return Ok(new { RspCode = "01", Message = "Order not found" });
                if (invoice.Status == InvoiceStatus.Paid) return Ok(new { RspCode = "02", Message = "Order already confirmed" });

                if (vnp_ResponseCode == "00")
                {
                    await _invoicePaymentService.MarkAsPaidAsync(invoice, "VNPay (IPN)", vnp_TransactionNo, "PaymentIPN");
                    _logger.LogInformation("[VNPay IPN] Invoice {Id} marked Paid.", invoiceId);
                }

                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPay IPN] Exception");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("payment/momo-ipn")]
        public async Task<IActionResult> MoMoIPN([FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("orderId", out var orderIdEl) ||
                    !body.TryGetProperty("resultCode", out var resultCodeEl) ||
                    !body.TryGetProperty("signature", out var signatureEl))
                {
                    return BadRequest("Missing required fields");
                }

                var orderId = orderIdEl.GetString() ?? "";
                var resultCode = resultCodeEl.GetInt32();
                var signature = signatureEl.GetString() ?? "";

                // Xác minh chữ ký HMAC SHA-256
                var secretKey = _configuration["MoMo:SecretKey"] ?? "";
                var rawSignatureData = $"orderId={orderId}&resultCode={resultCode}";
                var computedSig = ComputeHmacSHA256(secretKey, rawSignatureData);

                if (!computedSig.Equals(signature, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("[MoMo IPN] Invalid signature for orderId={OrderId}", orderId);
                    return Ok(new { status = "FAIL", message = "Invalid signature" });
                }

                var invoiceId = ParseInvoiceIdFromOrderId(orderId, "INV_");
                if (invoiceId == null)
                    return Ok(new { status = "FAIL", message = "Invalid orderId" });

                var invoice = await GetInvoiceWithDetailsAsync(i => i.Id == invoiceId.Value);
                if (invoice == null) return Ok(new { status = "FAIL", message = "Invoice not found" });
                if (invoice.Status == InvoiceStatus.Paid) return Ok(new { status = "OK", message = "Already paid" });

                if (resultCode == 0)  // MoMo: 0 = success
                {
                    await _invoicePaymentService.MarkAsPaidAsync(invoice, "MoMo (IPN)", orderId, "PaymentIPN");
                    _logger.LogInformation("[MoMo IPN] Invoice {Id} marked Paid.", invoiceId);
                }

                return Ok(new { status = "OK", message = "Received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MoMo IPN] Exception");
                return StatusCode(500);
            }
        }

        // ─── Private Helpers (DRY) ──────────────────────────────────────────

        /// <summary>
        /// Load invoice với đầy đủ navigation properties.
        /// Gộp query Include lặp 5+ lần → 1 method.
        /// </summary>
        private async Task<Invoice?> GetInvoiceWithDetailsAsync(System.Linq.Expressions.Expression<Func<Invoice, bool>> predicate)
        {
            return await _context.Invoices
                .Include(i => i.Contract)
                    .ThenInclude(c => c!.Room)
                        .ThenInclude(r => r!.Property)
                .FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Parse invoiceId từ orderId string. Hỗ trợ format "INV_123" hoặc "123_timestamp".
        /// </summary>
        private static int? ParseInvoiceIdFromOrderId(string? orderId, string? prefix = null, char splitChar = '_')
        {
            if (string.IsNullOrEmpty(orderId)) return null;
            if (prefix != null && !orderId.StartsWith(prefix)) return null;

            var parts = orderId.Split(splitChar);
            var idPart = prefix != null ? parts.ElementAtOrDefault(1) : parts.FirstOrDefault();
            return int.TryParse(idPart, out var id) ? id : null;
        }

        private static string ComputeHmacSHA256(string key, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash).ToLower();
        }
    }
}