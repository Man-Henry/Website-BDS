namespace Website_QLPT.Services.Ocr
{
    public class OcrResult
    {
        public bool Success { get; set; }
        public string? IdNumber { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Message { get; set; }
    }

    public class MeterOcrResult
    {
        public bool Success { get; set; }
        public decimal? Reading { get; set; }
        public string? Message { get; set; }
    }

    public interface IOcrService
    {
        Task<OcrResult> ExtractIdCardInfoAsync(IFormFile file);
        Task<MeterOcrResult> ExtractMeterReadingAsync(IFormFile file);
    }
}
