using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceAggregate.Exceptions;

namespace wfc.referential.Application.Services.Commands.DeleteService;

public class DeleteServiceCommandHandler(IServiceRepository serviceRepository,ITaxRuleDetailRepository taxRuleDetailRepository, ICacheService cacheService)
    : ICommandHandler<DeleteServiceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetOneByConditionAsync(p => p.Id == ServiceId.Of(request.ServiceId), cancellationToken);

        if (service is null)
            throw new ResourceNotFoundException("Service not found");

        var taxRuleDetails = await taxRuleDetailRepository.GetByConditionAsync(s => s.ServiceId == ServiceId.Of(request.ServiceId), cancellationToken);

        if (taxRuleDetails.Any()) throw new ServiceHasTaxesException(taxRuleDetails);

        service.SetInactive();
        serviceRepository.Update(service);

        await cacheService.RemoveByPrefixAsync(CacheKeys.Service.Prefix, cancellationToken);

        return Result.Success(true);
    }
}