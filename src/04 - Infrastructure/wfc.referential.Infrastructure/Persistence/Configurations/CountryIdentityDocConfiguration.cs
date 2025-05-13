using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wfc.referential.Domain.CountryIdentityDocAggregate;

namespace wfc.referential.Infrastructure.Persistence.Configurations;

public class CountryIdentityDocConfiguration : IEntityTypeConfiguration<CountryIdentityDoc>
{
    public void Configure(EntityTypeBuilder<CountryIdentityDoc> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new CountryIdentityDocId(value));

        builder.Property(x => x.CountryId)
            .HasConversion(
                id => id.Value,
                value => new Domain.Countries.CountryId(value))
            .IsRequired();

        builder.Property(x => x.IdentityDocumentId)
            .HasConversion(
                id => id.Value,
                value => new Domain.IdentityDocumentAggregate.IdentityDocumentId(value))
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Relations
        builder.HasOne<Domain.Countries.Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.IdentityDocumentAggregate.IdentityDocument>()
            .WithMany()
            .HasForeignKey(x => x.IdentityDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index pour recherche rapide d'association unique
        builder.HasIndex(x => new { x.CountryId, x.IdentityDocumentId }).IsUnique();
    }
}