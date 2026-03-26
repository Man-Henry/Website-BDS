namespace Website_QLPT.Services.Payment
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(int invoiceId, decimal amount, string contextInfo, string returnUrl);
        bool ValidateSignature(string rspraw, string inputHash);
    }
}
