using Mapster;
using wfc.referential.Application.Partners.Dtos;
using wfc.referential.Application.Services.Dtos;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Mappings;

public class ServiceMappings
{
    public static void Register()
    {
        TypeAdapterConfig<Service, GetAllServicesResponse>.NewConfig()
            .Map(dest => dest.ServiceId, src => src.Id!.Value)
            .Map(dest => dest.ProductId, src => src.ProductId.Value);

        // Map from ProductId to nullable Guid
        TypeAdapterConfig<ServiceId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        // Map from nullable Guid to ProductId
        TypeAdapterConfig<Guid?, ServiceId>
            .NewConfig()
            .MapWith(src => src.HasValue ? ServiceId.Of(src.Value) : null);
    }
}