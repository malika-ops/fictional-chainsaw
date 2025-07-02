using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class ServiceControleConfiguration : IEntityTypeConfiguration<ServiceControle>
{
    public void Configure(EntityTypeBuilder<ServiceControle> builder)
    {
        builder.ToTable("ServiceControles");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.Id)
               .HasConversion(id => id.Value,
                              guid => new ServiceControleId(guid));

        builder.Property(sc => sc.ServiceId)
               .HasConversion(id => id.Value, guid => new ServiceId(guid));

        builder.HasOne(sc => sc.Service)
               .WithMany()
               .HasForeignKey(sc => sc.ServiceId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Property(sc => sc.ControleId)
               .HasConversion(id => id.Value, guid => new ControleId(guid));

        builder.HasOne(sc => sc.Controle)
               .WithMany()
               .HasForeignKey(sc => sc.ControleId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Property(sc => sc.ChannelId)
               .HasConversion(id => id.Value, guid => new ParamTypeId(guid));

        builder.HasOne(sc => sc.Channel)
               .WithMany()
               .HasForeignKey(sc => sc.ChannelId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Property(sc => sc.ExecOrder)
               .IsRequired();

        builder.Property(sc => sc.IsEnabled)
               .IsRequired()
               .HasDefaultValue(true);

        builder.HasIndex(sc => new { sc.ServiceId, sc.ControleId, sc.ChannelId })
               .IsUnique();
    }
}