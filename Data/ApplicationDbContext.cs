using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Website_QLPT.Models;

namespace Website_QLPT.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<MaintenanceTicket> MaintenanceTickets { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AppNotification> AppNotifications { get; set; }
        public DbSet<LandlordProfile> LandlordProfiles { get; set; }
        public DbSet<RoomReview> RoomReviews { get; set; }
        public DbSet<PaymentConfig> PaymentConfigs { get; set; }
        public DbSet<UtilityTier> UtilityTiers { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ─── SOFT DELETE GLOBAL QUERY FILTERS ─────────────────────────────
            // Mọi query bình thường sẽ tự động lọc bỏ bản ghi đã xóa mềm.
            // Để đọc bản ghi đã xóa: .IgnoreQueryFilters()
            builder.Entity<Property>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Room>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Contract>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<MaintenanceRequest>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<ChatMessage>().HasQueryFilter(e => !e.IsDeleted);

            // ─── MATCHING FILTERS FOR DEPENDENT ENTITIES ────────────────────
            // Khi entity cha bị soft-delete, entity con cũng tự động bị lọc
            builder.Entity<RoomImage>().HasQueryFilter(e => !e.Room!.IsDeleted);
            builder.Entity<RoomReview>().HasQueryFilter(e => !e.Room!.IsDeleted);
            builder.Entity<MaintenanceTicket>().HasQueryFilter(e => !e.Contract!.IsDeleted);
            builder.Entity<UtilityTier>().HasQueryFilter(e => !e.Property!.IsDeleted);

            // ─── INDEXES ──────────────────────────────────────────────────────
            // Các index tối ưu query phổ biến nhất
            builder.Entity<Property>()
                .HasIndex(p => p.OwnerId)
                .HasDatabaseName("IX_Properties_OwnerId");

            builder.Entity<Room>()
                .HasIndex(r => r.PropertyId)
                .HasDatabaseName("IX_Rooms_PropertyId");

            builder.Entity<Invoice>()
                .HasIndex(i => i.ContractId)
                .HasDatabaseName("IX_Invoices_ContractId");

            builder.Entity<UtilityTier>()
                .HasIndex(u => new { u.PropertyId, u.Type })
                .HasDatabaseName("IX_UtilityTiers_PropertyId_Type");

            // ─── RELATIONSHIPS ─────────────────────────────────────────────────

            // Property -> IdentityUser (Owner)
            builder.Entity<Property>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Room -> Property
            builder.Entity<Room>()
                .HasOne(r => r.Property)
                .WithMany(p => p.Rooms)
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // RoomImage -> Room
            builder.Entity<RoomImage>()
                .HasOne(ri => ri.Room)
                .WithMany(r => r.Images)
                .HasForeignKey(ri => ri.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Contract -> Room
            builder.Entity<Contract>()
                .HasOne(c => c.Room)
                .WithMany(r => r.Contracts)
                .HasForeignKey(c => c.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contract -> Tenant
            builder.Entity<Contract>()
                .HasOne(c => c.Tenant)
                .WithMany(t => t.Contracts)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Tenant>()
                .HasOne(t => t.IdentityUser)
                .WithMany()
                .HasForeignKey(t => t.IdentityUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Tenant>()
                .HasIndex(t => t.IdentityUserId)
                .IsUnique()
                .HasFilter("[IdentityUserId] IS NOT NULL");

            // Invoice -> Contract
            builder.Entity<Invoice>()
                .HasOne(i => i.Contract)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Invoice>()
                .HasIndex(i => new { i.ContractId, i.Month, i.Year })
                .IsUnique();

            // PaymentConfig -> Owner
            builder.Entity<PaymentConfig>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PaymentConfig>()
                .HasIndex(p => new { p.OwnerId, p.Provider })
                .IsUnique();

            // UtilityTiers -> Property
            builder.Entity<UtilityTier>()
                .HasOne(u => u.Property)
                .WithMany(p => p.UtilityTiers)
                .HasForeignKey(u => u.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // ─── SOFT DELETE INTERCEPT ─────────────────────────────────────────────
        // Override SaveChanges để chuyển Hard Delete → Soft Delete tự động
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            InterceptSoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            InterceptSoftDelete();
            return base.SaveChanges();
        }

        private void InterceptSoftDelete()
        {
            var deletedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted && e.Entity is ISoftDelete);

            foreach (var entry in deletedEntries)
            {
                entry.State = EntityState.Modified;
                var softDelete = (ISoftDelete)entry.Entity;
                softDelete.IsDeleted = true;
                softDelete.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}
