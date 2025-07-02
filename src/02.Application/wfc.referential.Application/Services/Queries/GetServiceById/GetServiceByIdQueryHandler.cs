using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Services.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Queries.GetServiceById;

public class GetServiceByIdQueryHandler : IQueryHandler<GetServiceByIdQuery, GetServicesResponse>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServiceByIdQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<GetServicesResponse> Handle(GetServiceByIdQuery query, CancellationToken ct)
    {
        var id = ServiceId.Of(query.ServiceId);
        var entity = await _serviceRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Service with id '{query.ServiceId}' not found.");

        return entity.Adapt<GetServicesResponse>();
    }
} 