using Mapster;
using wfc.referential.Application.CountryIdentityDocs.Commands.CreateCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Dtos;
using wfc.referential.Application.CountryIdentityDocs.Queries.GetAllCountryIdentityDocs;
using wfc.referential.Domain.CountryIdentityDocAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Mappings;

public class CountryIdentityDocMappings
{
    public static void Register()
    {
        // Domain -> Response
        TypeAdapterConfig<CountryIdentityDoc, GetCountryIdentityDocResponse>
            .NewConfig()
            .Map(dest => dest.CountryIdentityDocId, src => src.Id.Value)
            .Map(dest => dest.CountryId, src => src.CountryId.Value)
            .Map(dest => dest.IdentityDocumentId, src => src.IdentityDocumentId.Value);

        // Request -> Command
        TypeAdapterConfig<CreateCountryIdentityDocRequest, CreateCountryIdentityDocCommand>
            .NewConfig()
            .ConstructUsing(src => new CreateCountryIdentityDocCommand(
                src.CountryId,
                src.IdentityDocumentId
            ));

        TypeAdapterConfig<UpdateCountryIdentityDocRequest, UpdateCountryIdentityDocCommand>
            .NewConfig()
            .ConstructUsing(src => new UpdateCountryIdentityDocCommand(
                src.CountryIdentityDocId,
                src.CountryId,
                src.IdentityDocumentId,
                src.IsEnabled
            ));

        TypeAdapterConfig<DeleteCountryIdentityDocRequest, DeleteCountryIdentityDocCommand>
            .NewConfig()
            .ConstructUsing(src => new DeleteCountryIdentityDocCommand(
                src.CountryIdentityDocId
            ));

        TypeAdapterConfig<PatchCountryIdentityDocRequest, PatchCountryIdentityDocCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .MapToConstructor(true)
            .ConstructUsing(src => new PatchCountryIdentityDocCommand(
                src.CountryIdentityDocId,
                src.CountryId,
                src.IdentityDocumentId,
                src.IsEnabled
            ));

        TypeAdapterConfig<PatchCountryIdentityDocCommand, CountryIdentityDoc>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<GetAllCountryIdentityDocsRequest, GetAllCountryIdentityDocsQuery>
            .NewConfig()
            .Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
            .Map(dest => dest.PageSize, src => src.PageSize ?? 10)
            .ConstructUsing(src => new GetAllCountryIdentityDocsQuery(
                src.PageNumber ?? 1,
                src.PageSize ?? 10,
                src.CountryId,
                src.IdentityDocumentId,
                src.IsEnabled
            ));
    }
}