using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Website_QLPT.Models;

namespace Website_QLPT.Services.Payment
{
    public class PayOSPaymentProvider : IPaymentProvider
    {
        public PaymentProvider ProviderType => PaymentProvider.PayOS;

        public string CreatePaymentUrl(Invoice invoice, PaymentConfig config, HttpContext context)
        {
            var configDict = JsonSerializer.Deserialize<Dictionary<string, string>>(config.ConfigData);
            if (configDict == null) throw new InvalidOperationException("Invalid PayOS config");

            string clientId = configDict.GetValueOrDefault("ClientId") ?? "";
            string apiKey = configDict.GetValueOrDefault("ApiKey") ?? "";
            string checksumKey = configDict.GetValueOrDefault("ChecksumKey") ?? "";

            string orderId = $"INV_{invoice.Id}_{DateTime.UtcNow.Ticks}";
            string amount = invoice.TotalAmount.ToString("0");
            string returnUrl = $"{context.Request.Scheme}://{context.Request.Host}/Payment/PayOSReturn";

            // In a real app we need to POST to PayOS endpoint (https://api-merchant.payos.vn/v2/payment-requests) 
            // and get the checkoutUrl.
            
            // Mock PayUrl for prototype:
            return $"/Payment/MockCheckout?provider=PayOS&amount={amount}&orderId={orderId}&returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        public Task<bool> ValidateWebhookAsync(HttpRequest request, PaymentConfig config)
        {
            // Verify PayOS signature
            return Task.FromResult(true);
        }
    }
}