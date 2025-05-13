using Mapster;
using wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Dtos;
using wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.TypeDefinitions.Mappings;

public class TypeDefinitionMappings
{
    public static void Register()
    {
        TypeAdapterConfig<GetAllTypeDefinitionsResponse, TypeDefinition>.NewConfig()
                .ConstructUsing(src => TypeDefinition.Create(
                    new TypeDefinitionId(src.TypeDefinitionId),
                    src.Libelle,
                    src.Description,
                    new List<ParamType>())
                );

        TypeAdapterConfig<TypeDefinition, GetAllTypeDefinitionsResponse>
            .NewConfig()
            .Map(dest => dest.TypeDefinitionId, src => src.Id.Value)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<GetAllTypeDefinitionsRequest, GetAllTypeDefinitionsQuery>
                .NewConfig()
                .ConstructUsing(src => new GetAllTypeDefinitionsQuery(
                    src.PageNumber ?? 1,
                    src.PageSize ?? 10,
                    src.Libelle,
                    src.Description,
                    src.IsEnabled
                ));

        TypeAdapterConfig<CreateTypeDefinitionRequest, CreateTypeDefinitionCommand>.NewConfig()
            .Map(dest => dest.Libelle, src => src.Libelle)
            .Map(dest => dest.Description, src => src.Description);

        TypeAdapterConfig<UpdateTypeDefinitionRequest, UpdateTypeDefinitionCommand>.NewConfig()
            .Map(dest => dest.TypeDefinitionId, src => src.TypeDefinitionId)
            .Map(dest => dest.Libelle, src => src.Libelle)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<PatchTypeDefinitionRequest, PatchTypeDefinitionCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .MapToConstructor(true)
            .ConstructUsing(src => new PatchTypeDefinitionCommand(
                src.TypeDefinitionId,
                src.Libelle,
                src.Description,
                src.IsEnabled
            ));

        TypeAdapterConfig<PatchTypeDefinitionCommand, TypeDefinition>
            .NewConfig()
            .IgnoreNullValues(true);
    }
}