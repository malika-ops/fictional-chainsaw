using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Taxes.Commands.DeleteTax;

public class DeleteTaxCommandHandler(
    ITaxRepository taxRepository,
    ICacheService cacheService
) : ICommandHandler<DeleteTaxCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteTaxCommand request, CancellationToken cancellationToken)
    {
        var tax = await taxRepository.GetByIdAsync(TaxId.Of(request.TaxId).Value, cancellationToken);

        if (tax is null)
            throw new ResourceNotFoundException($"{nameof(Tax)} not found");


        tax.SetInactive();
        await taxRepository.UpdateTaxAsync(tax, cancellationToken);

        await cacheService.SetAsync(request.CacheKey,tax,TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(true);
    }
}
