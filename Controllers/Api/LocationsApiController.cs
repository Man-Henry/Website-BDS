using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Website_QLPT.Controllers.Api
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/locations")]
    [ApiController]
    public class LocationsApiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<LocationsApiController> _logger;

        private const string OPEN_API_BASE_URL = "https://provinces.open-api.vn/api/";

        public LocationsApiController(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<LocationsApiController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            return await FetchAndCache("provinces", "p");
        }

        [HttpGet("districts/{provinceCode}")]
        public async Task<IActionResult> GetDistricts(int provinceCode)
        {
            return await FetchAndCache($"districts_{provinceCode}", $"p/{provinceCode}?depth=2", true);
        }

        [HttpGet("wards/{districtCode}")]
        public async Task<IActionResult> GetWards(int districtCode)
        {
            return await FetchAndCache($"wards_{districtCode}", $"d/{districtCode}?depth=2", true);
        }

        private async Task<IActionResult> FetchAndCache(string cacheKey, string endpoint, bool extractList = false)
        {
            if (_cache.TryGetValue(cacheKey, out string? cachedData) && cachedData != null)
            {
                return Content(cachedData, "application/json");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync(OPEN_API_BASE_URL + endpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch location data from Open API: {StatusCode}", response.StatusCode);
                    return StatusCode(502, new { message = "Lỗi kết nối tới hệ thống máy chủ địa lý VN." });
                }

                var content = await response.Content.ReadAsStringAsync();

                // Dữ liệu Tỉnh/Huyện trả về 1 Object chứa mảng danh sách bên trong thay vì array (tùy theo endpoint query ?depth=2)
                if (extractList)
                {
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("districts", out var districts))
                    {
                        content = districts.ToString();
                    }
                    else if (doc.RootElement.TryGetProperty("wards", out var wards))
                    {
                        content = wards.ToString();
                    }
                }

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24));
                _cache.Set(cacheKey, content, cacheOptions);

                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching locations data endpoint: {Endpoint}", endpoint);
                return StatusCode(500, new { message = "Lỗi hệ thống khi tải dữ liệu địa phương." });
            }
        }
    }
}
