using Microsoft.AspNetCore.Http;
using Website_QLPT.Models;

namespace Website_QLPT.Services.Payment
{
    public interface IPaymentProvider
    {
        PaymentProvider ProviderType { get; }
        
        /// <summary>
        /// Creates a payment URL to redirect the tenant to.
        /// </summary>
        string CreatePaymentUrl(Invoice invoice, PaymentConfig config, HttpContext context);

        /// <summary>
        /// Validates the webhook callback from the provider.
        /// Returns true if valid, false otherwise.
        /// </summary>
        Task<bool> ValidateWebhookAsync(HttpRequest request, PaymentConfig config);
    }
}