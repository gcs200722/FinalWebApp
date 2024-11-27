using FinalWebApp.Data.Configurations;
using FinalWebApp.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FinalWebApp.Data
{
    public class FinalWebDbContext : IdentityDbContext<ApplicationUser>
    {
        public FinalWebDbContext(DbContextOptions<FinalWebDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Đảm bảo các cấu hình mặc định của Identity được áp dụng

            // Áp dụng cấu hình cho các entity tùy chỉnh
            builder.ApplyConfiguration(new CategoryConfiguration());
            builder.ApplyConfiguration(new ItemConfiguration());
            builder.ApplyConfiguration(new UserConfiguration());
            builder.Entity<Order>()
            .HasOne(o => o.Customer)
       .WithMany() // Một khách hàng có thể có nhiều đơn hàng
       .HasForeignKey(o => o.CustomerId) // Khóa ngoại là CustomerId
       .OnDelete(DeleteBehavior.Restrict); // Hạn chế xóa
        }
        public DbSet<Category> Category { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }
    }
}
