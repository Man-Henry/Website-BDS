namespace Website_QLPT.Services.Email
{
    public interface IEmailTemplateService
    {
        string BuildInvoiceEmail(string tenantName, string roomName, string period, decimal amount, string actionUrl, string ctaText, string? statusNote = null);
    }
}
