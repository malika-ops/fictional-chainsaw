using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Taxes.Commands.DeleteTax;

public record DeleteTaxCommand : ICommand<Result<bool>>, ICacheableQuery
{
    public Guid TaxId { get; init; }
    public string CacheKey => $"{nameof(Tax)}_{TaxId}";
    public int CacheExpiration => 5;
}
