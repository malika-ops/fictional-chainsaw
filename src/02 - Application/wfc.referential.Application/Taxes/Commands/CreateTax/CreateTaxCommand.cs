using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Taxes.Commands.CreateTax;

public record CreateTaxCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public TaxId TaxId { get; init; } = TaxId.Of(Guid.NewGuid());
    public string Code { get; init; } = default!;
    public string CodeEn { get; init; } = default!;
    public string CodeAr { get; init; } = default!;
    public string Description { get; init; } = default!;
    public double FixedAmount { get; init; } = default!;
    public double Value { get; init; }
    public bool IsEnabled { get; init; } = true;

    public string CacheKey => $"{nameof(Tax)}_{TaxId}";
    public int CacheExpiration => 5;
}
