using Mapster;
using wfc.referential.Application.CountryIdentityDocs.Dtos;
using wfc.referential.Domain.CountryIdentityDocAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Mappings;

public class CountryIdentityDocMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CountryIdentityDocId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<CountryIdentityDoc, GetCountryIdentityDocsResponse>
            .NewConfig()
            .Map(d => d.CountryIdentityDocId, s => s.Id.Value)
            .Map(d => d.CountryId, s => s.CountryId.Value)
            .Map(d => d.IdentityDocumentId, s => s.IdentityDocumentId.Value);
    }
}