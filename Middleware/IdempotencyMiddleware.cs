using Microsoft.Extensions.Caching.Memory;

namespace Website_QLPT.Middleware
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<IdempotencyMiddleware> _logger;
        private const string HeaderName = "X-Idempotency-Key";

        public IdempotencyMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<IdempotencyMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only enforce idempotency on POST requests (usually payments or creations)
            if (HttpMethods.IsPost(context.Request.Method) && context.Request.Headers.TryGetValue(HeaderName, out var idempotencyKeyObj))
            {
                var idempotencyKey = idempotencyKeyObj.ToString();
                
                if (string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    await _next(context);
                    return;
                }

                // Append the route path to the key to distinguish requests to different endpoints
                var cacheKey = $"Idempotency_{idempotencyKey}_{context.Request.Path}";

                if (_cache.TryGetValue(cacheKey, out bool isProcessed))
                {
                    _logger.LogWarning("Idempotency hit! Prevented duplicate request for key: {Key} on path: {Path}", idempotencyKey, context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsJsonAsync(new { message = "Giao dịch đang được xử lý hoặc đã hoàn tất. Yêu cầu trùng lặp bị từ chối." });
                    return;
                }

                // Add to cache with an expiration window (e.g., 24 hours to prevent double-charging within the same day for same key)
                _cache.Set(cacheKey, true, TimeSpan.FromHours(24));
            }

            // Normal processing
            await _next(context);
        }
    }

    public static class IdempotencyMiddlewareExtensions
    {
        public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IdempotencyMiddleware>();
        }
    }
}
