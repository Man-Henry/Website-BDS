// ═══════════════════════════════════════════════════════════════════════════
// SeedData.cs — Tạo tài khoản mặc định khi khởi động ứng dụng
// ═══════════════════════════════════════════════════════════════════════════
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Website_QLPT.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // ─── ROLES ─────────────────────────────────────────────────────
            string[] roles = ["Admin", "Landlord", "Tenant"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ─── ADMIN USER ─────────────────────────────────────────────────
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            var adminEmail = config["AdminSeed:Email"]
                          ?? Environment.GetEnvironmentVariable("QLPT_ADMIN_EMAIL")
                          ?? (env.IsDevelopment() ? "admin@qlpt.dev" : null);
            var adminPassword = config["AdminSeed:Password"]
                             ?? Environment.GetEnvironmentVariable("QLPT_ADMIN_PASSWORD")
                             ?? (env.IsDevelopment() ? "Admin@123456" : null);

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                return;

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(adminUser, ["Admin", "Landlord"]);
                }
            }

            // ─── LANDLORD USER (chunha@gmail.com) ────────────────────────
            var customLandlordEmail = "chunha@gmail.com";
            var customLandlordUser = await userManager.FindByEmailAsync(customLandlordEmail);
            if (customLandlordUser == null)
            {
                customLandlordUser = new IdentityUser
                {
                    UserName = customLandlordEmail,
                    Email = customLandlordEmail,
                    EmailConfirmed = true
                };
                var lr = await userManager.CreateAsync(customLandlordUser, "19062004mM");
                if (lr.Succeeded)
                    await userManager.AddToRoleAsync(customLandlordUser, "Landlord");
            }

            // ─── TENANT USER (khachthue@gmail.com) ──────────────────────
            var customTenantEmail = "khachthue@gmail.com";
            var customTenantUser = await userManager.FindByEmailAsync(customTenantEmail);
            if (customTenantUser == null)
            {
                customTenantUser = new IdentityUser
                {
                    UserName = customTenantEmail,
                    Email = customTenantEmail,
                    EmailConfirmed = true
                };
                var tr = await userManager.CreateAsync(customTenantUser, "19062004mM");
                if (tr.Succeeded)
                    await userManager.AddToRoleAsync(customTenantUser, "Tenant");
            }

            // ─── Không tạo dữ liệu demo ─────────────────────────────────
            // Dữ liệu thực sẽ được nhập bởi người dùng qua giao diện
        }
    }
}
