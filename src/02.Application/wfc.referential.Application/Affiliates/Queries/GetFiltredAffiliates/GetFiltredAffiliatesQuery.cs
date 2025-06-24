using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Affiliates.Dtos;

namespace wfc.referential.Application.Affiliates.Queries.GetFiltredAffiliates;

public record GetFiltredAffiliatesQuery : IQuery<PagedResult<GetAffiliatesResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Code { get; init; }
    public string? Name { get; init; }
    public DateTime? OpeningDate { get; init; }
    public string? CancellationDay { get; init; }
    public Guid? CountryId { get; init; }
    public bool? IsEnabled { get; init; } = true;
}