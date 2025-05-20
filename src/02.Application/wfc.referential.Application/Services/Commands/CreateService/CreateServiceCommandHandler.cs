using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ServiceAggregate.Exceptions;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Commands.CreateService;

public class CreateServiceCommandHandler(IServiceRepository serviceRepository, ICacheService cacheService)
    : ICommandHandler<CreateServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var isExist = await serviceRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (isExist is not null)
            throw new CodeAlreadyExistException(request.Code);

        var service = Service.Create(
            ServiceId.Of(Guid.NewGuid()),
            request.Code,
            request.Name,
            request.IsEnabled,
            request.ProductId);

        await serviceRepository.AddServiceAsync(service, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, service, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(service.Id!.Value);
    }
}
