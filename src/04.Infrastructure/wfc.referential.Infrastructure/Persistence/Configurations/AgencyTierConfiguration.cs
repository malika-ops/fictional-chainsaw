using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class AgencyTierConfiguration : IEntityTypeConfiguration<AgencyTier>
{
    public void Configure(EntityTypeBuilder<AgencyTier> builder)
    {


        builder.ToTable("AgencyTiers");

        builder.HasKey(at => at.Id);

        builder.Property(at => at.Id)
               .HasConversion(id => id.Value,
                              guid => new AgencyTierId(guid))
               .HasValueGenerator((_, __) => new GuidValueGenerator())  
               .ValueGeneratedOnAdd();

        var agencyIdComparer = new ValueComparer<AgencyId>(
            (a, b) => a.Value == b.Value,
            a => a.Value.GetHashCode(),
            v => new AgencyId(v.Value));

        builder.Property(at => at.AgencyId)
               .HasConversion(id => id.Value, guid => new AgencyId(guid))
               .Metadata.SetValueComparer(agencyIdComparer);

        builder.HasOne(at => at.Agency)
               .WithMany()
               .HasForeignKey(at => at.AgencyId)        
               .OnDelete(DeleteBehavior.ClientSetNull);

        var tierIdComparer = new ValueComparer<TierId>(
            (a, b) => a.Value == b.Value,
            a => a.Value.GetHashCode(),
            v => new TierId(v.Value));

        builder.Property(at => at.TierId)
               .HasConversion(id => id.Value, guid => new TierId(guid))
               .Metadata.SetValueComparer(tierIdComparer);

        builder.HasOne(at => at.Tier)
               .WithMany()
               .HasForeignKey(at => at.TierId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Property(at => at.Code)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(t => t.Password)
               .HasMaxLength(500);

        builder.Property(t => t.IsEnabled)
               .HasDefaultValue(true)
               .IsRequired();

        builder.HasIndex(at => new { at.AgencyId, at.TierId, at.Code })
               .IsUnique();
    }
}
