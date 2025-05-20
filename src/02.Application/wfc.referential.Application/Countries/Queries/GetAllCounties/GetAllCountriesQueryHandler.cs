using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Countries.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Countries.Queries.GetAllCounties
{
    public class GetAllCountriesHandler : IQueryHandler<GetAllCountriesQuery, PagedResult<GetCountriesResponce>>
    {
        private readonly ICountryRepository _countryRepository;

        public GetAllCountriesHandler(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        public async Task<PagedResult<GetCountriesResponce>> Handle(GetAllCountriesQuery request, CancellationToken cancellationToken)
        {
            // Retrieve filtered and paginated list of countries.
            var countries = await _countryRepository.GetAllCountriesPaginatedAsyncFiltred(request, cancellationToken);
            int totalCount = await _countryRepository.GetCountTotalAsync(request, cancellationToken);

            var countryDtos = countries.Adapt<List<GetCountriesResponce>>();

            return new PagedResult<GetCountriesResponce>(countryDtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
