using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxAggregate.Exceptions;

namespace wfc.referential.Application.Taxes.Commands.UpdateTax;
public class UpdateTaxCommandHandler(ITaxRepository _taxRepository, ICacheService _cacheService) 
    : ICommandHandler<UpdateTaxCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateTaxCommand request, CancellationToken cancellationToken)
    {
        var taxId = TaxId.Of(request.TaxId);
        var tax = await _taxRepository.GetOneByConditionAsync(t => t.Id == taxId, cancellationToken);
        if (tax is null)
            throw new ResourceNotFoundException($"{nameof(Tax)} with ID {request.TaxId} not found.");

        var existingTaxWithSameCode = await _taxRepository.GetOneByConditionAsync(t => t.Code.Equals(request.Code), cancellationToken);
        if (existingTaxWithSameCode is not null && existingTaxWithSameCode.Id != tax.Id)
            throw new CodeAlreadyExistException(request.Code);

        tax.Update(request.Code, request.CodeAr, request.CodeEn, request.Description,
            request.FixedAmount, request.Rate, request.IsEnabled);

        _taxRepository.Update(tax);
        await _taxRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.Tax.Prefix, cancellationToken);

        return Result.Success(true);
    }
}

