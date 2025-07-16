using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceAggregate.Exceptions;

namespace wfc.referential.Application.Services.Commands.UpdateService;

public class UpdateServiceCommandHandler(IServiceRepository serviceRepository, ICacheService cacheService)
    : ICommandHandler<UpdateServiceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var entity = await serviceRepository.GetOneByConditionAsync(p => p.Id == ServiceId.Of(request.ServiceId), cancellationToken);
        if (entity is null)
            throw new ResourceNotFoundException("Service not found");

        var hasDuplicatedCode = await serviceRepository.GetOneByConditionAsync( c => c.Code == request.Code, cancellationToken);
        if (hasDuplicatedCode is not null) throw new CodeAlreadyExistException(request.Code);

        entity.Update(request.Code, request.Name,request.FlowDirection, request.IsEnabled, request.ProductId);

        serviceRepository.Update(entity);

        await cacheService.RemoveByPrefixAsync(CacheKeys.Service.Prefix, cancellationToken);

        return Result.Success(true);
    }
}