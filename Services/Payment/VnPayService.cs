using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;

namespace Website_QLPT.Services.Payment
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(int invoiceId, decimal amount, string contextInfo, string returnUrl)
        {
            var tmnCode = _configuration["VnPay:TmnCode"];
            var hashSecret = _configuration["VnPay:HashSecret"];
            var baseUrl = _configuration["VnPay:BaseUrl"];
            var version = _configuration["VnPay:Version"];

            var vnp_Params = new SortedList<string, string>
            {
                { "vnp_Version", version ?? "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode ?? "" },
                { "vnp_Amount", ((long)(amount * 100)).ToString() }, // Amount in VND * 100
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", "127.0.0.1" }, // Dummy IP for simple implementation
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", $"Thanh toan hoa don {invoiceId}: {contextInfo}" },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", $"{invoiceId}_{DateTime.Now.Ticks}" } // Unique TxnRef
            };

            var signData = string.Join("&", vnp_Params.Select(kv => $"{kv.Key}={HttpUtility.UrlEncode(kv.Value)}"));
            var vnp_SecureHash = HmacSHA512(hashSecret ?? "", signData);

            var paymentUrl = $"{baseUrl}?{signData}&vnp_SecureHash={vnp_SecureHash}";
            return paymentUrl;
        }

        public bool ValidateSignature(string rspraw, string inputHash)
        {
            var hashSecret = _configuration["VnPay:HashSecret"];
            var myChecksum = HmacSHA512(hashSecret ?? "", rspraw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }
}
