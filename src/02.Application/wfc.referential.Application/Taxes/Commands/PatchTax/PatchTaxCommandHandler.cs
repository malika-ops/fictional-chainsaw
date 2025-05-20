using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Taxes.Commands.PatchTax;

public class PatchTaxCommandHandler(ITaxRepository taxRepository, ICacheService cacheService) 
    : ICommandHandler<PatchTaxCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchTaxCommand request, CancellationToken cancellationToken)
    {
        var tax = await taxRepository.GetByIdAsync(request.TaxId, cancellationToken);
        if (tax is null)
            throw new ResourceNotFoundException($"{nameof(Tax)} with ID {request.TaxId} not found.");

        request.Adapt(tax);
        tax.Patch(); 

        await taxRepository.UpdateTaxAsync(tax, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, tax, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(tax.Id!.Value);
    }
}
