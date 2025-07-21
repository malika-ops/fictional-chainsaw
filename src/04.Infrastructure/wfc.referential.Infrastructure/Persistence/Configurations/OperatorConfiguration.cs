using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
    public void Configure(EntityTypeBuilder<Operator> builder)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                Id => Id.Value,
                value => new OperatorId(value));

        // Required properties
        builder.Property(o => o.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.IdentityCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Numéro de la carte d'identité");

        builder.Property(o => o.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(o => o.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Enum for OperatorType
        builder.Property(o => o.OperatorType)
            .HasConversion<int>()
            .IsRequired(false)
            .HasComment("Agence, filiale, partenaire, prestataire, regional, sectoriel, siege WFC, sous-réseau");

        // Foreign key to Branch 
        builder.Property(o => o.BranchId)
            .IsRequired(false);

        // TODO: ProfileId - La table profile n'existe pas encore
        // builder.Property(o => o.ProfileId)
        //     .IsRequired(false)
        //     .HasComment("La table profile n'existe pas");

        // Unique indexes
        builder.HasIndex(o => o.Code)
            .IsUnique();

        builder.HasIndex(o => o.IdentityCode)
            .IsUnique();

        builder.HasIndex(o => o.Email)
            .IsUnique();

        // Table name
        builder.ToTable("Operators");
    }
}