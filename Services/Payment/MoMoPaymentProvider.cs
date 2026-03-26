using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Website_QLPT.Models;

namespace Website_QLPT.Services.Payment
{
    public class MoMoPaymentProvider : IPaymentProvider
    {
        public PaymentProvider ProviderType => PaymentProvider.MoMo;

        public string CreatePaymentUrl(Invoice invoice, PaymentConfig config, HttpContext context)
        {
            // Parse config
            var configDict = JsonSerializer.Deserialize<Dictionary<string, string>>(config.ConfigData);
            if (configDict == null) throw new InvalidOperationException("Invalid MoMo config");

            string partnerCode = configDict.GetValueOrDefault("PartnerCode") ?? "";
            string accessKey = configDict.GetValueOrDefault("AccessKey") ?? "";
            string secretKey = configDict.GetValueOrDefault("SecretKey") ?? "";
            string endpoint = configDict.GetValueOrDefault("Endpoint") ?? "https://test-payment.momo.vn/v2/gateway/api/create";

            string orderId = $"INV_{invoice.Id}_{DateTime.UtcNow.Ticks}";
            string amount = invoice.TotalAmount.ToString("0");
            string orderInfo = $"Thanh toán hóa đơn tháng {invoice.Month}/{invoice.Year}";
            string returnUrl = $"{context.Request.Scheme}://{context.Request.Host}/Payment/MoMoReturn";
            string notifyUrl = $"{context.Request.Scheme}://{context.Request.Host}/api/webhooks/momo/{config.OwnerId}";
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            // signature
            string rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType=captureWallet";
            string signature = SignHmacSHA256(rawHash, secretKey);

            // In a real app we need to POST to MoMo endpoint and get the payUrl.
            // For this prototype, we'll construct a mock checkout URL if POST fails.
            
            // Because making an actual HTTP request to MoMo might fail with fake keys,
            // we will simulate the PayUrl redirection directly for the sake of the prototype.
            // A real integration would do:
            // var requestData = new { partnerCode, accessKey, requestId, amount, orderId, orderInfo, returnUrl, notifyUrl, requestType="captureWallet", signature, extraData };
            // var response = await httpClient.PostAsJsonAsync(endpoint, requestData);
            // var responseData = await response.Content.ReadFromJsonAsync<MoMoResponse>();
            // return responseData.payUrl;

            // Mock PayUrl:
            return $"/Payment/MockCheckout?provider=MoMo&amount={amount}&orderId={orderId}&returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        public Task<bool> ValidateWebhookAsync(HttpRequest request, PaymentConfig config)
        {
            // Read MoMo signature from request, validate it against SecretKey
            // For prototype, return true
            return Task.FromResult(true);
        }

        private string SignHmacSHA256(string message, string secretKey)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                string hex = BitConverter.ToString(hashmessage);
                hex = hex.Replace("-", "").ToLower();
                return hex;
            }
        }
    }
}