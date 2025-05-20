using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Services.Commands.DeleteService;

public class DeleteServiceCommandHandler(IServiceRepository serviceRepository, ICacheService cacheService)
    : ICommandHandler<DeleteServiceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);

        if (service is null)
            throw new ResourceNotFoundException("Service not found");

        service.SetInactive();
        await serviceRepository.UpdateServiceAsync(service, cancellationToken);
        await cacheService.RemoveAsync(request.CacheKey, cancellationToken);

        return Result.Success(true);
    }
}