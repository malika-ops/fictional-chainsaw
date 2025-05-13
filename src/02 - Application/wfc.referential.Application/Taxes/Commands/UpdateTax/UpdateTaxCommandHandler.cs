using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxAggregate.Exceptions;

namespace wfc.referential.Application.Taxes.Commands.UpdateTax;
public class UpdateTaxCommandHandler(
    ITaxRepository _taxRepository,
    ICacheService _cacheService
) : ICommandHandler<UpdateTaxCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateTaxCommand request, CancellationToken cancellationToken)
    {
        var tax = await _taxRepository.GetByIdAsync(request.TaxId, cancellationToken);
        if (tax is null)
            throw new ResourceNotFoundException($"{nameof(Tax)} with ID {request.TaxId} not found.");

        var existingTaxWithSameCode = await _taxRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existingTaxWithSameCode is not null && existingTaxWithSameCode.Id != tax.Id)
            throw new CodeAlreadyExistException(request.Code);

        request.Adapt(tax);
        tax.Update();

        await _taxRepository.UpdateTaxAsync(tax, cancellationToken);

        await _cacheService.SetAsync(
            request.CacheKey,
            tax,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken
        );

        return Result.Success(tax.Id!.Value);
    }
}

