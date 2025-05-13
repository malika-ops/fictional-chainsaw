using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ServiceAggregate.Exceptions;

namespace wfc.referential.Application.Services.Commands.UpdateService;

public class UpdateServiceCommandHandler(IServiceRepository serviceRepository, ICacheService cacheService)
    : ICommandHandler<UpdateServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var entity = await serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
        if (entity is null)
            throw new ResourceNotFoundException("Service not found");

        var hasDuplicatedCode = await serviceRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (hasDuplicatedCode is not null) throw new CodeAlreadyExistException(request.Code);

        entity.Update(request.Code, request.Name, request.IsEnabled, request.ProductId);

        await serviceRepository.UpdateServiceAsync(entity, cancellationToken);
        await cacheService.RemoveAsync(request.CacheKey, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}