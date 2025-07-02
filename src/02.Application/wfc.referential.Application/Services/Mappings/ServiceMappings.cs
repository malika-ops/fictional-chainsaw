using Mapster;
using wfc.referential.Application.Services.Dtos;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Mappings;

public class ServiceMappings
{
    public static void Register()
    {
        TypeAdapterConfig<Service, GetServicesResponse>.NewConfig()
            .Map(dest => dest.ServiceId, src => src.Id!.Value)
            .Map(dest => dest.ProductId, src => src.ProductId.Value);

        // Map from ProductId to nullable Guid
        TypeAdapterConfig<ServiceId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        TypeAdapterConfig<ServiceId, Guid>
       .NewConfig()
       .MapWith(src => src.Value);
    }
}