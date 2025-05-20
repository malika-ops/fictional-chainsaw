using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;
public class ParamTypeConfiguration : IEntityTypeConfiguration<ParamType>
{
    public void Configure(EntityTypeBuilder<ParamType> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                        Id => Id.Value,
                        value => new ParamTypeId(value));

        builder.Property(r => r.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);
    }
}