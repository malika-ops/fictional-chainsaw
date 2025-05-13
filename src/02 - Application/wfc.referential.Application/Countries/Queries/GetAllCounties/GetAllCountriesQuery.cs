using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Countries.Dtos;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Countries.Queries.GetAllCounties
{
    public record GetAllCountriesQuery : IQuery<PagedResult<GetCountriesResponce>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? Name { get; init; }
        public string? Code { get; init; }
        public string? ISO2 { get; init; }
        public string? ISO3 { get; init; }
        public string? DialingCode { get; init; }
        public string? TimeZone { get; init; }
        public bool? HasSector { get; init; }
        public bool? IsSmsEnabled { get; init; }
        public int? NumberDecimalDigits { get; init; }
        public bool? IsEnabled { get; init; }
        public string? Abbreviation { get; init; }
        public string CacheKey => $"{nameof(Country)}_page{PageNumber}_size{PageSize}_code{Name}_name{Code}_status{IsEnabled}";
        public int CacheExpiration => 5;

    }
}
