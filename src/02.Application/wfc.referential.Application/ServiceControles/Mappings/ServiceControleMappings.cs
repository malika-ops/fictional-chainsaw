using Mapster;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceControleAggregate;

namespace wfc.referential.Application.ServiceControles.Mappings;

public class ServiceControleMappings
{
    public static void Register()
    {
        TypeAdapterConfig<ServiceControleId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);


        // Nested mappings 
        TypeAdapterConfig<ParamType, ParamTypesResponse>.NewConfig()
            .Map(dest => dest.ParamTypeId, src => src.Id!.Value);

    }
}
