using Mapster;
using wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Commands.DeleteTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Dtos;
using wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.TypeDefinitions.Mappings;

public class TypeDefinitionMappings
{
    public static void Register()
    {
        TypeAdapterConfig<GetAllTypeDefinitionsRequest, GetAllTypeDefinitionsQuery>.NewConfig()
            .ConstructUsing(src => new GetAllTypeDefinitionsQuery(
                src.PageNumber ?? 1,
                src.PageSize ?? 10,
                src.Libelle,
                src.Description,
                src.IsEnabled
            ));

        TypeAdapterConfig<CreateTypeDefinitionRequest, CreateTypeDefinitionCommand>
            .NewConfig()
            .ConstructUsing(src => new CreateTypeDefinitionCommand(src.Libelle, src.Description));

        TypeAdapterConfig<DeleteTypeDefinitionRequest, DeleteTypeDefinitionCommand>
            .NewConfig()
            .ConstructUsing(src => new DeleteTypeDefinitionCommand(src.TypeDefinitionId));

        TypeAdapterConfig<UpdateTypeDefinitionRequest, UpdateTypeDefinitionCommand>
            .NewConfig()
            .ConstructUsing(src => new UpdateTypeDefinitionCommand(src.TypeDefinitionId, src.Libelle, src.Description, src.IsEnabled));

        TypeAdapterConfig<PatchTypeDefinitionRequest, PatchTypeDefinitionCommand>
            .NewConfig()
            .ConstructUsing(src => new PatchTypeDefinitionCommand(src.TypeDefinitionId, src.Libelle, src.Description, src.IsEnabled));

        TypeAdapterConfig<PatchTypeDefinitionCommand, TypeDefinition>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<TypeDefinition, GetAllTypeDefinitionsResponse>
           .NewConfig()
           .Map(dest => dest.TypeDefinitionId, src => src.Id.Value);
    }
}