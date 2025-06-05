using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceAggregate.Exceptions;

namespace wfc.referential.Application.Services.Commands.PatchService;

public class PatchServiceCommandHandler(IServiceRepository serviceRepository, ICacheService cacheService)
    : ICommandHandler<PatchServiceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PatchServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetOneByConditionAsync(p => p.Id == ServiceId.Of(request.ServiceId), cancellationToken);

        if (service is null)
            throw new ResourceNotFoundException($"{nameof(Service)} not found");

        var duplicatedCode = await serviceRepository.GetOneByConditionAsync(p => p.Code.Equals(request.Code), cancellationToken);

        if (duplicatedCode is not null)
            throw new CodeAlreadyExistException($"{nameof(Product)} not found");

        service.Patch(request.Code,request.Name,request.IsEnabled,ProductId.Of(request.ProductId));

        serviceRepository.Update(service);

        await cacheService.RemoveByPrefixAsync(CacheKeys.Service.Prefix, cancellationToken);

        return Result.Success(true);
    }
}