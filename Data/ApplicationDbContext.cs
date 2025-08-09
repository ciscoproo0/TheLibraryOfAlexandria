using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Models;

namespace TheLibraryOfAlexandria.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ShippingInfo> ShippingInfos { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(
                    v => v.ToString(),  // Converts UserRole to database
                    v => (UserRole)Enum.Parse(typeof(UserRole), v)); // Converts string back to enum when reading from database
            modelBuilder.Entity<Payment>()
        .Property(p => p.Method)
        .HasConversion(
            v => v.ToString(),
            v => (PaymentMethod)Enum.Parse(typeof(PaymentMethod), v));

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v));

            // Resilient conversion for ShippingStatus (tolerates null/invalid strings)
            modelBuilder.Entity<ShippingInfo>()
                .Property(s => s.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => string.IsNullOrWhiteSpace(v)
                        ? ShippingStatus.Preparing
                        : v.ToLower() == "preparing" ? ShippingStatus.Preparing
                        : v.ToLower() == "shipped" ? ShippingStatus.Shipped
                        : v.ToLower() == "delivered" ? ShippingStatus.Delivered
                        : v.ToLower() == "returned" ? ShippingStatus.Returned
                        : v.ToLower() == "cancelled" ? ShippingStatus.Cancelled
                        : ShippingStatus.Preparing)
                .HasDefaultValue(ShippingStatus.Preparing);

            // 1:1 relationship Order -> ShippingInfo with FK on ShippingInfo.OrderId
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShippingInfo)
                .WithOne()
                .HasForeignKey<ShippingInfo>(s => s.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
