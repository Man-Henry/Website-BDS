using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Website_QLPT.Services.Ocr;

namespace Website_QLPT.Controllers.Api
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/ocr")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class OcrApiController : ControllerBase
    {
        private readonly IOcrService _ocrService;

        public OcrApiController(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        [HttpPost("scan-id")]
        public async Task<IActionResult> ScanIdCard(IFormFile file)
        {
            try
            {
                var result = await _ocrService.ExtractIdCardInfoAsync(file);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi phân tích ảnh: " + ex.Message });
            }
        }

        [HttpPost("scan-meter")]
        public async Task<IActionResult> ScanMeter(IFormFile file)
        {
            try
            {
                var result = await _ocrService.ExtractMeterReadingAsync(file);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi phân tích ảnh đồng hồ: " + ex.Message });
            }
        }
    }
}
