using FinalWebApp.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FinalWebApp.Data.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name)
        .IsRequired()
        .HasMaxLength(75);
            builder.Property(c => c.Description)
       .HasMaxLength(300);
            builder.Property(c => c.Price)
        .IsRequired()
         .HasPrecision(18, 2);
        }
    }
}
