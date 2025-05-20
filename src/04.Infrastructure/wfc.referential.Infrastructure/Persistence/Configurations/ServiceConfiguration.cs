using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => new ServiceId(value));

        builder.Property(r => r.Code).IsRequired();
        builder.Property(r => r.Name).IsRequired();

        builder.HasIndex(r => r.Code).IsUnique();

        builder.Property(r => r.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.ProductId)
            .HasConversion(
                id => id.Value,
                value => new ProductId(value));

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(r => r.LastModified)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}