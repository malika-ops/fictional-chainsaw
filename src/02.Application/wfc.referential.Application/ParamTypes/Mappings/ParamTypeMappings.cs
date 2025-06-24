using Mapster;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Mappings;

public class ParamTypeMappings
{
    public static void Register()
    {
        TypeAdapterConfig<ParamTypeId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<ParamType, GetFiltredParamTypesResponse>
            .NewConfig()
            .Map(d => d.ParamTypeId, s => s.Id.Value);

        TypeAdapterConfig<Guid, TypeDefinitionId>
            .NewConfig()
            .MapWith(src => TypeDefinitionId.Of(src));
    }
}