using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Queries.GetFiltredTaxeRuleDetails;

public record GetFiltredTaxRuleDetailsQuery : IQuery<PagedResult<GetTaxRuleDetailsResponse>>, ICacheableQuery
{
    public Guid? CorridorId { get; init; }
    public Guid? TaxId { get; init; }
    public Guid? ServiceId { get; init; }
    public ApplicationRule? AppliedOn { get; init; }
    public bool? IsEnabled { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string CacheKey =>
        $"{nameof(TaxRuleDetail)}_Corridor{CorridorId}_Tax{TaxId}_Service{ServiceId}_AppliedOn{AppliedOn}_Enabled{IsEnabled}_Page{PageNumber}_Size{PageSize}";

    public int CacheExpiration => 5;
}