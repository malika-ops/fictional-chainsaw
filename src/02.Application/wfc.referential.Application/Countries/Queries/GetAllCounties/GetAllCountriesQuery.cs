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
        public bool? IsEnabled { get; init; }

    }
}
