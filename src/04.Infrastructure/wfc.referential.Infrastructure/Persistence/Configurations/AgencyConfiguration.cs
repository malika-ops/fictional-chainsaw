using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

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

        builder.Property(a => a.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(a => a.Code).IsUnique();

        builder.Property(a => a.Name).IsRequired().HasMaxLength(300);
        builder.Property(a => a.Abbreviation).IsRequired().HasMaxLength(200);

        builder.Property(a => a.Address1).IsRequired().HasMaxLength(500);
        builder.Property(a => a.Address2).HasMaxLength(500);

        builder.Property(a => a.Phone).HasMaxLength(20);
        builder.Property(a => a.Fax).HasMaxLength(30);

        builder.Property(a => a.AccountingSheetName).HasMaxLength(300);
        builder.Property(a => a.AccountingAccountNumber).HasMaxLength(300);

        builder.Property(a => a.PostalCode).HasMaxLength(30);

        builder.Property(a => a.CashTransporter).HasMaxLength(200);                
        builder.Property(a => a.ExpenseFundAccountingSheet).HasMaxLength(300);     
        builder.Property(a => a.ExpenseFundAccountNumber).HasMaxLength(300);      
        builder.Property(a => a.MadAccount).HasMaxLength(300);                     

        builder.Property(a => a.FundingThreshold)
               .HasColumnType("numeric(18,3)");                                  

        builder.Property(a => a.Latitude).HasColumnType("numeric");
        builder.Property(a => a.Longitude).HasColumnType("numeric");

        builder.Property(a => a.IsEnabled)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(a => a.AgencyTypeId)
               .HasConversion(id => id.Value, guid => new ParamTypeId(guid));

        builder.Property(a => a.TokenUsageStatusId)                                
               .HasConversion(id => id.Value, guid => new ParamTypeId(guid));

        builder.Property(a => a.FundingTypeId)                                    
               .HasConversion(id => id.Value, guid => new ParamTypeId(guid));

        builder.Property(a => a.CityId)
               .HasConversion(id => id.Value, guid => new CityId(guid));

        builder.Property(a => a.SectorId)
               .HasConversion(id => id.Value, guid => new SectorId(guid));

        builder.Property(a => a.PartnerId)                                        
               .HasConversion(id => id.Value, guid => new PartnerId(guid));

        builder.Property(a => a.SupportAccountId)                                 
               .HasConversion(id => id.Value, guid => new SupportAccountId(guid));


        builder.HasOne(a => a.AgencyType)
               .WithMany()
               .HasForeignKey(a => a.AgencyTypeId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(a => a.TokenUsageStatus)                                  
               .WithMany()
               .HasForeignKey(a => a.TokenUsageStatusId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(a => a.FundingType)                                        
               .WithMany()
               .HasForeignKey(a => a.FundingTypeId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(a => a.City)
               .WithMany()
               .HasForeignKey(a => a.CityId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(a => a.Sector)
               .WithMany()
               .HasForeignKey(a => a.SectorId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(a => a.Partner)                                             
               .WithMany()
               .HasForeignKey(a => a.PartnerId)
               .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(a => a.SupportAccount)                                     
               .WithMany()
               .HasForeignKey(a => a.SupportAccountId)
               .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
