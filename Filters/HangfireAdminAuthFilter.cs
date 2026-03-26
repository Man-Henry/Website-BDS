using Hangfire.Dashboard;

namespace Website_QLPT.Filters
{
    /// <summary>
    /// Cho phép truy cập Hangfire Dashboard chỉ khi đã đăng nhập với Role "Admin".
    /// Thay thế LocalRequestsOnlyAuthorizationFilter (chỉ hoạt động ở localhost).
    /// </summary>
    public class HangfireAdminAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Phải đăng nhập
            if (httpContext.User.Identity?.IsAuthenticated != true)
                return false;

            // Phải có Role Admin
            return httpContext.User.IsInRole("Admin");
        }
    }
}
