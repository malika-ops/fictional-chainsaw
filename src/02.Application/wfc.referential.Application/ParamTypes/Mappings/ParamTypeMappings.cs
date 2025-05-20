using Mapster;
using wfc.referential.Application.ParamTypes.Commands.CreateParamType;
using wfc.referential.Application.ParamTypes.Commands.UpdateParamType;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Application.ParamTypes.Queries.GetAllParamTypes;
using wfc.referential.Application.TypeDefinitions.Dtos;
using wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Mappings;

public class ParamTypeMappings
{
    public static void Register()
    {
        TypeAdapterConfig<GetAllParamTypesRequest, GetAllParamTypesQuery>.NewConfig()
            .ConstructUsing(src => new GetAllParamTypesQuery(
                src.PageNumber ?? 1,
                src.PageSize ?? 10,
                src.Value,
                TypeDefinitionId.Of(src.TypeDefinitionId),
                src.IsEnabled
            ));

        TypeAdapterConfig<CreateParamTypeRequest, CreateParamTypeCommand>.NewConfig()
                .ConstructUsing(src => new CreateParamTypeCommand(
                    new ParamTypeId(Guid.NewGuid()),
                    src.Value,
                    TypeDefinitionId.Of(src.TypeDefinitionId)
                ));

        //ParamType
        TypeAdapterConfig<GetAllParamTypesResponse, ParamType>.NewConfig()
            .ConstructUsing(src => ParamType.Create(
                new ParamTypeId(src.ParamTypeId),
                src.TypeDefinitionId,
                src.Value
            ));

        TypeAdapterConfig<UpdateParamTypeRequest, UpdateParamTypeCommand>.NewConfig()
            .ConstructUsing(src => new UpdateParamTypeCommand(
                new ParamTypeId(src.ParamTypeId),
                src.Value,
                src.IsEnabled,
                TypeDefinitionId.Of(src.TypeDefinitionId)
            ));

        TypeAdapterConfig<ParamType, GetAllParamTypesResponse>
            .NewConfig()
            .Map(dest => dest.ParamTypeId, src => src.Id.Value)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);
    }
}