using Mapster;
using wfc.referential.Application.Services.Commands.CreateService;
using wfc.referential.Application.Services.Commands.DeleteService;
using wfc.referential.Application.Services.Commands.PatchService;
using wfc.referential.Application.Services.Commands.UpdateService;
using wfc.referential.Application.Services.Dtos;
using wfc.referential.Application.Services.Queries.GetAllServices;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Mappings;

public class ServiceMappings
{
    public static void Register()
    {
        TypeAdapterConfig<GetAllServicesRequest, GetAllServicesQuery>
        .NewConfig()
        .Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
        .Map(dest => dest.PageSize, src => src.PageSize ?? 10)
        .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<Service, GetAllServicesResponse>.NewConfig()
            .Map(dest => dest.ServiceId, src => src.Id!.Value)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled.ToString())
            .Map(dest => dest.ProductId, src => src.ProductId.Value);

        TypeAdapterConfig<CreateServiceRequest, CreateServiceCommand>.NewConfig()
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ProductId, src => ProductId.Of(src.ProductId));

        TypeAdapterConfig<PatchServiceRequest, PatchServiceCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .Map(dest => dest.ServiceId, src => src.ServiceId)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<PatchServiceCommand, Service>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<DeleteServiceRequest, DeleteServiceCommand>.NewConfig()
            .Map(dest => dest.ServiceId, src => src.ServiceId);

        TypeAdapterConfig<UpdateServiceRequest, UpdateServiceCommand>.NewConfig()
            .Map(dest => dest.ServiceId, src => src.ServiceId)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled)
            .Map(dest => dest.ProductId, src => ProductId.Of(src.ProductId));

        TypeAdapterConfig<ServiceId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        TypeAdapterConfig<Guid?, ServiceId>
            .NewConfig()
            .MapWith(src => src.HasValue ? ServiceId.Of(src.Value) : null);
    }
}