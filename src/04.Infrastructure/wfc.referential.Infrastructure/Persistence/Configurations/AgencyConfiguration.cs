using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("Agencies", tb =>
        {
            tb.HasCheckConstraint(
                "CK_Agency_CityOrSector",
                @"((""CityId"" IS NOT NULL AND ""SectorId"" IS NULL) OR
           (""CityId"" IS NULL     AND ""SectorId"" IS NOT NULL))");
        });

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
               .HasConversion(id => id.Value, guid => new AgencyId(guid));

        builder.Property(a => a.Code).IsRequired();
        builder.Property(a => a.Name).IsRequired();
        builder.Property(a => a.Abbreviation).IsRequired().HasMaxLength(200);

        builder.Property(a => a.Address1).IsRequired();
        builder.Property(a => a.Address2);

        builder.Property(a => a.Phone).HasMaxLength(20);
        builder.Property(a => a.Fax).HasMaxLength(30);

        builder.Property(a => a.AccountingSheetName).HasMaxLength(300);
        builder.Property(a => a.AccountingAccountNumber).HasMaxLength(300);

        builder.Property(a => a.MoneyGramReferenceNumber).HasMaxLength(300);
        builder.Property(a => a.MoneyGramPassword).HasMaxLength(300);

        builder.Property(a => a.PostalCode).HasMaxLength(30);
        builder.Property(a => a.PermissionOfficeChange).HasMaxLength(300);


        builder.Property(t => t.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(a => a.Code).IsUnique();


        builder.Property(a => a.AgencyTypeId)
               .HasConversion(id => id.Value, guid => new ParamTypeId(guid));

        builder.Property(a => a.CityId)
               .HasConversion(id => id.Value, guid => new CityId(guid));

        builder.Property(a => a.SectorId)
               .HasConversion(id => id.Value, guid => new SectorId(guid));

        builder.HasOne(a => a.AgencyType)
               .WithMany()
               .HasForeignKey(a => a.AgencyTypeId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(a => a.City)
               .WithMany()
               .HasForeignKey(a => a.CityId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(a => a.Sector)
               .WithMany()
               .HasForeignKey(a => a.SectorId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Property(a => a.Latitude).HasColumnType("numeric");
        builder.Property(a => a.Longitude).HasColumnType("numeric");
    }
}
