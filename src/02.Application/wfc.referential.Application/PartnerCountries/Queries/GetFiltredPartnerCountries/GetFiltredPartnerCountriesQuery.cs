using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.PartnerCountries.Dtos;


namespace wfc.referential.Application.PartnerCountries.Queries.GetFiltredPartnerCountries;

public record GetFiltredPartnerCountriesQuery : IQuery<PagedResult<PartnerCountryResponse>>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public Guid? PartnerId { get; init; }
    public Guid? CountryId { get; init; }
    public bool? IsEnabled { get; init; }
}