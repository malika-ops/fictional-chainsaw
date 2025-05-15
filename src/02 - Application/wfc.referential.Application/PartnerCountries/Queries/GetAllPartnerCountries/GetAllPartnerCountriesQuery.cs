using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.PartnerCountries.Dtos;


namespace wfc.referential.Application.PartnerCountries.Queries.GetAllPartnerCountries;

public record GetAllPartnerCountriesQuery : IQuery<PagedResult<PartnerCountryResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? PartnerId { get; init; }
    public Guid? CountryId { get; init; }
    public bool? IsEnabled { get; init; } = true;
}