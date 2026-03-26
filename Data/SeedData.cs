// ═══════════════════════════════════════════════════════════════════════════
// SeedData.cs — Demo data tự động khi ASPNETCORE_ENVIRONMENT == Development
// ═══════════════════════════════════════════════════════════════════════════
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Website_QLPT.Models;

namespace Website_QLPT.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();

            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

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
            // Credentials đọc từ environment variables hoặc appsettings.json
            // Production: set QLPT_ADMIN_EMAIL và QLPT_ADMIN_PASSWORD
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var adminEmail = config["AdminSeed:Email"]
                          ?? Environment.GetEnvironmentVariable("QLPT_ADMIN_EMAIL")
                          ?? (env.IsDevelopment() ? "admin@qlpt.dev" : null);
            var adminPassword = config["AdminSeed:Password"]
                             ?? Environment.GetEnvironmentVariable("QLPT_ADMIN_PASSWORD")
                             ?? (env.IsDevelopment() ? "Admin@123456" : null);

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                return; // Production không có credentials → skip

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

            // ─── DEMO DATA (Development only) ──────────────────────────────
            if (!env.IsDevelopment()) return;

            // Kiểm tra nếu đã có data thì skip
            if (await context.Properties.AnyAsync()) return;

            // ── Property + Rooms ──────────────────────────────────────────
            var property = new Property
            {
                Name = "Nhà Trọ Demo — 123 Nguyễn Văn Linh",
                Address = "123 Nguyễn Văn Linh, Phường Tân Phong, Quận 7, TP.HCM",
                Description = "Nhà trọ demo cho môi trường development",
                OwnerId = adminUser.Id,
                Latitude = 10.7202,
                Longitude = 106.7177,
                IsElectricityTiered = true
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            // ── EVN Bậc thang (6 bậc) ────────────────────────────────────
            var evnTiers = new List<UtilityTier>
            {
                new UtilityTier { PropertyId = property.Id, Type = UtilityType.Electricity, FromUnit = 0, ToUnit = 50, Price = 1806 },
                new UtilityTier { PropertyId = property.Id, Type = UtilityType.Electricity, FromUnit = 51, ToUnit = 100, Price = 1866 },
                new UtilityTier { PropertyId = property.Id, Type = UtilityType.Electricity, FromUnit = 101, ToUnit = 200, Price = 2167 },
                new UtilityTier { PropertyId = property.Id, Type = UtilityType.Electricity, FromUnit = 201, ToUnit = 300, Price = 2729 },
                new UtilityTier { PropertyId = property.Id, Type = UtilityType.Electricity, FromUnit = 301, ToUnit = 400, Price = 3050 },
                new UtilityTier { PropertyId = property.Id, Type = UtilityType.Electricity, FromUnit = 401, ToUnit = null, Price = 3151 }
            };
            context.UtilityTiers.AddRange(evnTiers);
            await context.SaveChangesAsync();

            var rooms = new List<Room>
            {
                new Room { Name = "P.101", Area = 20, Price = 3_500_000, Status = RoomStatus.Rented,   PropertyId = property.Id },
                new Room { Name = "P.102", Area = 22, Price = 3_800_000, Status = RoomStatus.Rented,   PropertyId = property.Id },
                new Room { Name = "P.103", Area = 18, Price = 3_200_000, Status = RoomStatus.Available, PropertyId = property.Id }
            };
            context.Rooms.AddRange(rooms);
            await context.SaveChangesAsync();

            // ── Tenants ───────────────────────────────────────────────────
            var tenant1Email = "tenant1@qlpt.dev";
            var tenant1User = await userManager.FindByEmailAsync(tenant1Email);
            if (tenant1User == null)
            {
                tenant1User = new IdentityUser { UserName = tenant1Email, Email = tenant1Email, EmailConfirmed = true };
                await userManager.CreateAsync(tenant1User, "Tenant@123456");
                await userManager.AddToRoleAsync(tenant1User, "Tenant");
            }

            var tenant2Email = "tenant2@qlpt.dev";
            var tenant2User = await userManager.FindByEmailAsync(tenant2Email);
            if (tenant2User == null)
            {
                tenant2User = new IdentityUser { UserName = tenant2Email, Email = tenant2Email, EmailConfirmed = true };
                await userManager.CreateAsync(tenant2User, "Tenant@123456");
                await userManager.AddToRoleAsync(tenant2User, "Tenant");
            }

            var tenant1 = new Tenant { FullName = "Nguyễn Văn An", PhoneNumber = "0901234567", Email = tenant1Email, OwnerId = adminUser.Id, IdentityUserId = tenant1User.Id };
            var tenant2 = new Tenant { FullName = "Trần Thị Bình", PhoneNumber = "0912345678", Email = tenant2Email, OwnerId = adminUser.Id, IdentityUserId = tenant2User.Id };
            context.Tenants.AddRange(tenant1, tenant2);
            await context.SaveChangesAsync();

            // ── Contracts ─────────────────────────────────────────────────
            var contract1 = new Contract
            {
                RoomId = rooms[0].Id, TenantId = tenant1.Id,
                StartDate = DateTime.Today.AddMonths(-3),
                EndDate = DateTime.Today.AddMonths(9),
                DepositAmount = 3_500_000, Status = ContractStatus.Active
            };
            var contract2 = new Contract
            {
                RoomId = rooms[1].Id, TenantId = tenant2.Id,
                StartDate = DateTime.Today.AddMonths(-1),
                EndDate = DateTime.Today.AddDays(20),   // Sắp hết hạn → trigger reminder
                DepositAmount = 3_800_000, Status = ContractStatus.Active
            };
            context.Contracts.AddRange(contract1, contract2);
            await context.SaveChangesAsync();

            // ── Invoices (tháng hiện tại với chỉ số điện/nước) ───────────
            var now = DateTime.Now;
            var invoice1 = new Invoice
            {
                ContractId = contract1.Id,
                Month = now.Month, Year = now.Year,
                RoomFee = 3_500_000,
                ElectricityOld = 100, ElectricityNew = 145, ElectricityPrice = 3500, // Sử dụng 45kWh (1 bậc)
                WaterOld = 20, WaterNew = 26, WaterPrice = 15000,
                Status = InvoiceStatus.Unpaid
            };
            
            // Tính tay vì SeedData không nên inject service
            invoice1.WaterFee = (26 - 20) * 15000;
            // 45kWh < 50kWh → Bậc 1 (1806đ)
            invoice1.ElectricityFee = 45 * 1806;
            invoice1.ElectricityCalculationDetails = "45 kWh x 1,806đ = 81,270đ";

            var invoice2 = new Invoice
            {
                ContractId = contract2.Id,
                Month = now.Month, Year = now.Year,
                RoomFee = 3_800_000,
                ElectricityOld = 200, ElectricityNew = 258, ElectricityPrice = 3500, // Sử dụng 58kWh (nửa bậc 1, nửa bậc 2)
                WaterOld = 10, WaterNew = 14, WaterPrice = 15000,
                Status = InvoiceStatus.Unpaid
            };
            invoice2.WaterFee = (14 - 10) * 15000;
            // 58kWh → 50kWh bậc 1 (1806) + 8kWh bậc 2 (1866)
            invoice2.ElectricityFee = (50 * 1806) + (8 * 1866);
            invoice2.ElectricityCalculationDetails = "50 kWh x 1,806đ = 90,300đ | 8 kWh x 1,866đ = 14,928đ";

            context.Invoices.AddRange(invoice1, invoice2);
            await context.SaveChangesAsync();
        }
    }
}
