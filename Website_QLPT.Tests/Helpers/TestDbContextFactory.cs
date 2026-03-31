using Microsoft.EntityFrameworkCore;
using Website_QLPT.Data;
using Website_QLPT.Models;

namespace Website_QLPT.Tests.Helpers
{
    /// <summary>
    /// Factory tạo InMemory DbContext cho unit tests
    /// </summary>
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext Create(string? dbName = null)
        {
            dbName ??= Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>
        /// Tạo DbContext + seed basic test data
        /// </summary>
        public static ApplicationDbContext CreateWithSeedData(string? dbName = null)
        {
            var context = Create(dbName);
            SeedTestData(context);
            return context;
        }

        private static void SeedTestData(ApplicationDbContext context)
        {
            // Properties
            var prop1 = new Property
            {
                Id = 1,
                Name = "Test Property 1",
                Address = "123 Test Street",
                Description = "Test property",
                OwnerId = "admin-user-id",
                Latitude = 10.7202,
                Longitude = 106.7177
            };
            var prop2 = new Property
            {
                Id = 2,
                Name = "Test Property 2",
                Address = "456 Test Ave",
                Description = "Second test property",
                OwnerId = "admin-user-id",
                Latitude = 10.8471,
                Longitude = 106.7717
            };
            context.Properties.AddRange(prop1, prop2);

            // Rooms
            var rooms = new List<Room>
            {
                new Room { Id = 1, Name = "P.101", Area = 20, Price = 3_500_000, Status = RoomStatus.Rented, PropertyId = 1 },
                new Room { Id = 2, Name = "P.102", Area = 22, Price = 3_800_000, Status = RoomStatus.Available, PropertyId = 1 },
                new Room { Id = 3, Name = "P.103", Area = 18, Price = 3_200_000, Status = RoomStatus.Available, PropertyId = 1 },
                new Room { Id = 4, Name = "P.201", Area = 25, Price = 4_000_000, Status = RoomStatus.Rented, PropertyId = 2 },
                new Room { Id = 5, Name = "P.202", Area = 20, Price = 3_500_000, Status = RoomStatus.Available, PropertyId = 2 }
            };
            context.Rooms.AddRange(rooms);

            // Tenants
            var tenant1 = new Tenant { Id = 1, FullName = "Nguyễn Văn An", PhoneNumber = "0901234567", Email = "tenant1@test.dev", OwnerId = "admin-user-id", IdentityUserId = "tenant1-id" };
            var tenant2 = new Tenant { Id = 2, FullName = "Trần Thị Bình", PhoneNumber = "0912345678", Email = "tenant2@test.dev", OwnerId = "admin-user-id", IdentityUserId = "tenant2-id" };
            context.Tenants.AddRange(tenant1, tenant2);

            // Contracts
            var contract1 = new Contract
            {
                Id = 1, RoomId = 1, TenantId = 1,
                StartDate = DateTime.Today.AddMonths(-3),
                EndDate = DateTime.Today.AddMonths(9),
                DepositAmount = 3_500_000, Status = ContractStatus.Active
            };
            var contract2 = new Contract
            {
                Id = 2, RoomId = 4, TenantId = 2,
                StartDate = DateTime.Today.AddMonths(-1),
                EndDate = DateTime.Today.AddMonths(11),
                DepositAmount = 4_000_000, Status = ContractStatus.Active
            };
            context.Contracts.AddRange(contract1, contract2);

            // Invoices
            var now = DateTime.Now;
            context.Invoices.AddRange(
                new Invoice
                {
                    Id = 1, ContractId = 1,
                    Month = now.Month, Year = now.Year,
                    RoomFee = 3_500_000,
                    ElectricityOld = 100, ElectricityNew = 145, ElectricityPrice = 3500,
                    ElectricityFee = 45 * 1806,
                    WaterOld = 20, WaterNew = 26, WaterPrice = 15000,
                    WaterFee = 6 * 15000,
                    Status = InvoiceStatus.Unpaid
                },
                new Invoice
                {
                    Id = 2, ContractId = 2,
                    Month = now.Month, Year = now.Year,
                    RoomFee = 4_000_000,
                    ElectricityOld = 50, ElectricityNew = 80, ElectricityPrice = 3500,
                    ElectricityFee = 30 * 1806,
                    WaterOld = 5, WaterNew = 10, WaterPrice = 15000,
                    WaterFee = 5 * 15000,
                    Status = InvoiceStatus.Unpaid
                }
            );

            context.SaveChanges();
        }
    }
}
