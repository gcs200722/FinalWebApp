using FinalWebApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinalWebApp.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Đặt độ dài tối đa cho Fullname
            builder
                   .Property(u => u.Fullname)
                   .HasMaxLength(100)
                   .IsRequired(true);  // Bạn có thể chỉnh thành `IsRequired(true)` nếu Fullname là bắt buộc

            // Đặt độ dài tối đa cho Avatar (đường dẫn ảnh)
            builder
                   .Property(u => u.Avatar)
                   .HasMaxLength(255)
                   .IsRequired(false); // Nếu Avatar không bắt buộc, có thể bỏ .IsRequired()

            // Đặt độ dài tối đa cho số điện thoại
            builder
                   .Property(u => u.NumberPhone)
                   .HasMaxLength(15)
                   .IsRequired(true); // Điều này phụ thuộc vào yêu cầu số điện thoại có bắt buộc không

            // Đảm bảo rằng Email sẽ có độ dài tối đa là 256 ký tự
            builder
                   .Property(u => u.Email)
                   .HasMaxLength(256)
                   .IsRequired(true); // Đặt `IsRequired(true)` cho Email nếu bắt buộc

            // Nếu muốn chắc chắn ngày sinh phải được nhập, bạn có thể sử dụng IsRequired cho DateOfBirth
            builder
                   .Property(u => u.DateOfBirth)
                   .IsRequired(false); // Chỉnh sửa nếu muốn trường này là bắt buộc
        }
    }
}
