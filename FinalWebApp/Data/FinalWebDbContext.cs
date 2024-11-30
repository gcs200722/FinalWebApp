using FinalWebApp.Data.Configurations;
using FinalWebApp.Data.Dto;
using FinalWebApp.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        // Cấu hình Order - Customer
        builder.Entity<Order>()
         .HasOne(o => o.Customer)
         .WithMany()
         .HasForeignKey(o => o.CustomerId)
         .OnDelete(DeleteBehavior.Restrict); // Hạn chế xóa

        // Cấu hình OrderItem - Category
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Category)
            .WithMany(c => c.OrderItems) // Chú ý "OrderItems" thay vì "Items"
            .HasForeignKey(oi => oi.CategoryId)
            .OnDelete(DeleteBehavior.NoAction); // Sử dụng NO ACTION thay vì CASCADE

        // Cấu hình OrderItem - Item
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Item)
            .WithMany(i => i.OrderItems) // Đảm bảo có thuộc tính "OrderItems" trong Item
            .HasForeignKey(oi => oi.ItemId)
            .OnDelete(DeleteBehavior.NoAction); // Sử dụng NO ACTION thay vì CASCADE

        // Cấu hình Price với kiểu dữ liệu chính xác
        builder.Entity<OrderItem>()
            .Property(oi => oi.Price)
            .HasColumnType("decimal(18,2)"); // Đảm bảo độ chính xác và tỷ lệ cho Price
    }

    public DbSet<Category> Categories { get; set; }  // Sử dụng tên số nhiều cho DbSet
    public DbSet<Item> Items { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<CustomerReview> CustomerReviews { get; set; }  // Thêm bảng CustomerReview cho phản hồi của khách hàng
}
