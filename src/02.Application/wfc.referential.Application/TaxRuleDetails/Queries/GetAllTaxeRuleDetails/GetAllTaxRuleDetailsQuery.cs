using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Queries.GetAllTaxeRuleDetails;

public record GetAllTaxRuleDetailsQuery : IQuery<PagedResult<GetAllTaxRuleDetailsResponse>>, ICacheableQuery
{
    public CorridorId? CorridorId { get; init; }
    public TaxId? TaxId { get; init; }
    public ServiceId? ServiceId { get; init; }
    public ApplicationRule? AppliedOn { get; init; }
    public bool? IsEnabled { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string CacheKey =>
        $"{nameof(TaxRuleDetail)}_Corridor{CorridorId?.Value}_Tax{TaxId?.Value}_Service{ServiceId?.Value}_AppliedOn{AppliedOn}_Enabled{IsEnabled}_Page{PageNumber}_Size{PageSize}";

    public int CacheExpiration => 5;
}