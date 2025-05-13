using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxAggregate.Exceptions;

namespace wfc.referential.Application.Taxes.Commands.CreateTax;

public class CreateTaxCommandHandler(
    ITaxRepository taxRepository,
    ICacheService cacheService
) : ICommandHandler<CreateTaxCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTaxCommand request, CancellationToken cancellationToken)
    {
        var isExist = await taxRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (isExist is not null)
            throw new CodeAlreadyExistException(request.Code);

        var tax = Tax.Create(
            request.TaxId,
            request.Code,
            request.CodeEn,
            request.CodeAr,
            request.Description,
            request.FixedAmount,
            request.Value
        );

        await taxRepository.AddTaxAsync(tax, cancellationToken);

        await cacheService.SetAsync(
            request.CacheKey,
            tax,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken
        );

        return Result.Success(tax.Id!.Value);
    }
}
