using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxAggregate.Exceptions;

namespace wfc.referential.Application.Taxes.Commands.DeleteTax;

public class DeleteTaxCommandHandler(
    ITaxRepository taxRepository,
    ICacheService cacheService
) : ICommandHandler<DeleteTaxCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteTaxCommand request, CancellationToken cancellationToken)
    {
        var taxId = TaxId.Of(request.TaxId);
        var tax = await taxRepository.GetByIdAsync(request.TaxId, cancellationToken);

        if (tax is null)
            throw new ResourceNotFoundException($"{nameof(Tax)} not found");

        if (await taxRepository.HasTaxRuleDetailsAsync(taxId, cancellationToken))
            throw new TaxHasTaxRuleDetailsException(request.TaxId);

        tax.SetInactive();
        await taxRepository.UpdateTaxAsync(tax, cancellationToken);

        await cacheService.SetAsync(request.CacheKey,tax,TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(true);
    }
}
