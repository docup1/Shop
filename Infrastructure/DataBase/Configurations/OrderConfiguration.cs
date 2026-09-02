using Infrastructure.DataBase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataBase.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(o => o.SenderCity)
            .HasColumnName("sender_city")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(o => o.RecipientCity)
            .HasColumnName("recipient_city")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(o => o.SenderAddress)
            .HasColumnName("sender_address")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(o => o.RecipientAddress)
            .HasColumnName("recipient_address")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(o => o.Weight)
            .HasColumnName("weight")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status");

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasIndex(o => o.RecipientCity);
    }
}
