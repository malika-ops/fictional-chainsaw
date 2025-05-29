using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxAggregate.Exceptions;

namespace wfc.referential.Application.Taxes.Commands.DeleteTax;

public class DeleteTaxCommandHandler(ITaxRepository taxRepository, 
    ICacheService _cacheService, ITaxRuleDetailRepository taxRuleDetailRepository) 
    : ICommandHandler<DeleteTaxCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteTaxCommand request, CancellationToken cancellationToken)
    {
        var taxId = TaxId.Of(request.TaxId);
        var tax = await taxRepository.GetOneByConditionAsync(t => t.Id == taxId, cancellationToken);

        if (tax is null)
            throw new ResourceNotFoundException($"{nameof(Tax)} not found");

        var taxRuleDetails = await taxRuleDetailRepository.GetByConditionAsync(trd => trd.TaxId == taxId, cancellationToken);
        if (taxRuleDetails.Any())
            throw new TaxHasTaxRuleDetailsException(request.TaxId);

        tax.SetInactive();
        taxRepository.Update(tax);
        await taxRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.Tax.Prefix, cancellationToken);

        return Result.Success(true);
    }
}
