using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Taxes.Commands.PatchTax;

public class PatchTaxCommandHandler(ITaxRepository taxRepository, ICacheService _cacheService) 
    : ICommandHandler<PatchTaxCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PatchTaxCommand request, CancellationToken cancellationToken)
    {
        var taxId = TaxId.Of(request.TaxId);
        var tax = await taxRepository.GetOneByConditionAsync(t => t.Id == taxId, cancellationToken);
        if (tax is null)
            throw new ResourceNotFoundException($"{nameof(Tax)} with ID {request.TaxId} not found.");

        tax.Patch(request.Code, request.CodeAr, request.CodeEn, request.Description,
            request.FixedAmount, request.Rate, request.IsEnabled); 

        taxRepository.Update(tax);
        await taxRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.Tax.Prefix, cancellationToken);

        return Result.Success(true);
    }
}
