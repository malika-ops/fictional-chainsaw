using Mapster;
using wfc.referential.Application.TypeDefinitions.Dtos;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.TypeDefinitions.Mappings;

public class TypeDefinitionMappings
{
    public static void Register()
    {
        TypeAdapterConfig<TypeDefinitionId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<TypeDefinition, GetTypeDefinitionsResponse>
            .NewConfig()
            .Map(d => d.TypeDefinitionId, s => s.Id.Value);
    }
}