using Website_QLPT.Services.Ocr;

namespace Website_QLPT.Services.Ocr
{
    public class DummyOcrService : IOcrService
    {
        public async Task<OcrResult> ExtractIdCardInfoAsync(IFormFile file)
        {
            // Simulate network/processing delay
            await Task.Delay(1500);

            if (file == null || file.Length == 0)
            {
                return new OcrResult { Success = false, Message = "File không hợp lệ." };
            }

            // In a real app, you would send the file to FPT.AI, Zalo AI, or Google Vision.
            // Here we just return mock data for demonstration.
            return new OcrResult
            {
                Success = true,
                IdNumber = "079099" + new Random().Next(100000, 999999).ToString(),
                FullName = "NGUYỄN VĂN A (TỪ ẢNH)",
                Address = "123 Đường Số 1, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh"
            };
        }

        public async Task<MeterOcrResult> ExtractMeterReadingAsync(IFormFile file)
        {
            await Task.Delay(1000);

            if (file == null || file.Length == 0)
            {
                return new MeterOcrResult { Success = false, Message = "File không hợp lệ." };
            }

            // Fake random reading from 100 to 5000
            return new MeterOcrResult
            {
                Success = true,
                Reading = new Random().Next(100, 5000)
            };
        }
    }
}
