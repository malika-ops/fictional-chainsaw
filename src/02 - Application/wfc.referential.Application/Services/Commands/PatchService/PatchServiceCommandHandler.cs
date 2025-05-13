using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Commands.PatchService;

public class PatchServiceCommandHandler(IServiceRepository serviceRepository, ICacheService cacheService)
    : ICommandHandler<PatchServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);

        if (service is null)
            throw new ResourceNotFoundException($"{nameof(Service)} not found");

        request.Adapt(service);
        service.Patch();
        await serviceRepository.UpdateServiceAsync(service, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, service, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(service.Id!.Value);
    }
}