using Mapster;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Mappings;

public class IdentityDocumentMappings
{
    public static void Register()
    {
        TypeAdapterConfig<IdentityDocumentId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<IdentityDocument, GetIdentityDocumentsResponse>
            .NewConfig()
            .Map(d => d.IdentityDocumentId, s => s.Id.Value);
    }
}