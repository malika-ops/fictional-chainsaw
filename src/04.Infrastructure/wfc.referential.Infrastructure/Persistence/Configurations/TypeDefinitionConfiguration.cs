using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;
public class TypeDefinitionConfiguration : IEntityTypeConfiguration<TypeDefinition>
{
    public void Configure(EntityTypeBuilder<TypeDefinition> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                        Id => Id.Value,
                        value => new TypeDefinitionId(value));

        builder.Property(r => r.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasMany(td => td.ParamTypes)
               .WithOne(pt => pt.TypeDefinition)
               .HasForeignKey(pt => pt.TypeDefinitionId)
               .OnDelete(DeleteBehavior.ClientSetNull);
    }
}