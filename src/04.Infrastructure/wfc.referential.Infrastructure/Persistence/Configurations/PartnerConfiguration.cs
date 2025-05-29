using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                Id => Id.Value,
                value => new PartnerId(value));

        builder.Property(p => p.Code).IsRequired();
        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.PersonType);
        builder.Property(p => p.ProfessionalTaxNumber);
        builder.Property(p => p.WithholdingTaxRate);
        builder.Property(p => p.HeadquartersCity);
        builder.Property(p => p.HeadquartersAddress);
        builder.Property(p => p.LastName);
        builder.Property(p => p.FirstName);
        builder.Property(p => p.PhoneNumberContact);
        builder.Property(p => p.MailContact);
        builder.Property(p => p.FunctionContact);
        builder.Property(p => p.TransferType);
        builder.Property(p => p.AuthenticationMode);
        builder.Property(p => p.TaxIdentificationNumber);
        builder.Property(p => p.TaxRegime);
        builder.Property(p => p.AuxiliaryAccount);
        builder.Property(p => p.ICE);
        builder.Property(p => p.Logo);
        builder.Property(p => p.IdParent);

        builder.Property(p => p.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // ParamType relationships
        builder.Property(p => p.NetworkModeId)
            .HasConversion(
                Id => Id!.Value,
                value => new ParamTypeId(value))
            .IsRequired(false);

        builder.Property(p => p.PaymentModeId)
            .HasConversion(
                Id => Id!.Value,
                value => new ParamTypeId(value))
            .IsRequired(false);

        builder.Property(p => p.PartnerTypeId)
            .HasConversion(
                Id => Id!.Value,
                value => new ParamTypeId(value))
            .IsRequired(false);

        builder.Property(p => p.SupportAccountTypeId)
            .HasConversion(
                Id => Id!.Value,
                value => new ParamTypeId(value))
            .IsRequired(false);

        // Foreign key relationships to ParamType
        builder.HasOne(p => p.NetworkMode)
            .WithMany()
            .HasForeignKey(p => p.NetworkModeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PaymentMode)
            .WithMany()
            .HasForeignKey(p => p.PaymentModeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PartnerType)
            .WithMany()
            .HasForeignKey(p => p.PartnerTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.SupportAccountType)
            .WithMany()
            .HasForeignKey(p => p.SupportAccountTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique indexes
        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.TaxIdentificationNumber).IsUnique();
        builder.HasIndex(p => p.ICE).IsUnique();

        // Account relationships
        builder.Property(p => p.CommissionAccountId);
        builder.Property(p => p.ActivityAccountId);
        builder.Property(p => p.SupportAccountId);

        // Ignore navigation properties
        builder.Ignore(p => p.CommissionAccount);
        builder.Ignore(p => p.ActivityAccount);
        builder.Ignore(p => p.SupportAccount);
    }
}