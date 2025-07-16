using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceAggregate.Exceptions;

namespace wfc.referential.Application.Services.Commands.CreateService;

public class CreateServiceCommandHandler(IServiceRepository serviceRepository,IProductRepository productRepository, ICacheService cacheService)
    : ICommandHandler<CreateServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var isExist = await serviceRepository.GetOneByConditionAsync(p => p.Code.Equals(request.Code), cancellationToken);
        if (isExist is not null)
            throw new CodeAlreadyExistException(request.Code);

        var product = await productRepository.GetByIdAsync(ProductId.Of(request.ProductId), cancellationToken);
        if (product is null)
            throw new BusinessException($"Product with ID {request.ProductId} not found");

        var service = Service.Create(
            ServiceId.Of(Guid.NewGuid()),
            request.Code,
            request.Name,
            request.FlowDirection,
            request.IsEnabled,
            ProductId.Of(request.ProductId));

        await serviceRepository.AddAsync(service, cancellationToken);
        await serviceRepository.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveByPrefixAsync(CacheKeys.Service.Prefix, cancellationToken);

        return Result.Success(service.Id!.Value);
    }
}
