using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                Id => Id.Value,
                value => new ProductId(value));

        builder.Property(r => r.Name).IsRequired();

        builder.Property(r => r.Code).IsRequired();

        builder.HasIndex(r => r.Code).IsUnique();

        builder.Property(p => p.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(r => r.LastModified)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
