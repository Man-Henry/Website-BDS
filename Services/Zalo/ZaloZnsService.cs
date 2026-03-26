using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Website_QLPT.Models;

namespace Website_QLPT.Services.Zalo
{
    public class ZaloOaOptions
    {
        public string AccessToken { get; set; } = string.Empty;
        public string InvoiceTemplateId { get; set; } = string.Empty;
    }

    public interface IZaloZnsService
    {
        Task<bool> SendInvoiceNotificationAsync(Tenant tenant, Invoice invoice);
    }

    public class ZaloZnsService : IZaloZnsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ZaloZnsService> _logger;
        private readonly ZaloOaOptions _options;

        public ZaloZnsService(HttpClient httpClient, IOptions<ZaloOaOptions> options, ILogger<ZaloZnsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<bool> SendInvoiceNotificationAsync(Tenant tenant, Invoice invoice)
        {
            if (string.IsNullOrEmpty(tenant.PhoneNumber))
            {
                _logger.LogWarning("Tenant {TenantId} does not have a phone number. Cannot send ZNS.", tenant.Id);
                return false;
            }

            try
            {
                // Format phone number to standard Zalo format 84...
                var znsPhone = tenant.PhoneNumber.StartsWith("0") 
                    ? "84" + tenant.PhoneNumber.Substring(1) 
                    : tenant.PhoneNumber;

                var payload = new
                {
                    phone = znsPhone,
                    template_id = _options.InvoiceTemplateId,
                    template_data = new
                    {
                        tenant_name = tenant.FullName,
                        invoice_month = $"{invoice.Month}/{invoice.Year}",
                        total_amount = invoice.TotalAmount.ToString("N0") + " VND",
                        due_date = invoice.CreatedAt.AddDays(5).ToString("dd/MM/yyyy")
                    }
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("access_token", _options.AccessToken);

                var response = await _httpClient.PostAsJsonAsync("message/template", payload);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ZaloZnsResponse>();
                    if (result != null && result.Error == 0)
                    {
                        _logger.LogInformation("Successfully sent ZNS invoice notification to {Phone}", znsPhone);
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Zalo API error: {ErrorMessage} (Code: {ErrorCode})", result?.Message, result?.Error);
                        return false;
                    }
                }
                else
                {
                    var errContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send ZNS. Http Status: {Status}, Content: {Content}", response.StatusCode, errContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending Zalo ZNS notification to {Phone}", tenant.PhoneNumber);
                // The Caller (or Polly circuit breaker) can handle/throw this exception
                throw;
            }
        }
    }

    public class ZaloZnsResponse
    {
        public int Error { get; set; }
        public string? Message { get; set; }
    }
}
