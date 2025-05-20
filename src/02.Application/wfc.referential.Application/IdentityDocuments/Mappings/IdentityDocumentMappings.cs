using Mapster;
using wfc.referential.Application.IdentityDocuments.Commands;
using wfc.referential.Application.IdentityDocuments.Commands.CreateIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Commands.DeleteIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Commands.UpdateIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Mappings;

public class IdentityDocumentMappings
{
    public static void Register()
    {
        TypeAdapterConfig<GetAllIdentityDocumentsRequest, GetAllIdentityDocumentsQuery>
            .NewConfig()
            .Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
            .Map(dest => dest.PageSize, src => src.PageSize ?? 10)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<IdentityDocument, GetAllIdentityDocumentsResponse>
            .NewConfig()
            .Map(dest => dest.IdentityDocumentId, src => src.Id.Value)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled.ToString());

        TypeAdapterConfig<CreateIdentityDocumentRequest, CreateIdentityDocumentCommand>
            .NewConfig()
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description);

        TypeAdapterConfig<UpdateIdentityDocumentRequest, UpdateIdentityDocumentCommand>
            .NewConfig()
            .Map(dest => dest.IdentityDocumentId, src => src.IdentityDocumentId)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<PatchIdentityDocumentRequest, PatchIdentityDocumentCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .Map(dest => dest.IdentityDocumentId, src => src.IdentityDocumentId)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<DeleteIdentityDocumentRequest, DeleteIdentityDocumentCommand>
            .NewConfig()
            .Map(dest => dest.IdentityDocumentId, src => src.IdentityDocumentId);
    }
}