namespace Website_QLPT.ViewModels
{
    public class QRPaymentViewModel
    {
        public int InvoiceId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string BankId { get; set; } = string.Empty;
    }
}
