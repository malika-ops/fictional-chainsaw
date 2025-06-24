using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Countries.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Countries.Queries.GetFiltredCounties
{
    public class GetFiltredCountriesHandler : IQueryHandler<GetFiltredCountriesQuery, PagedResult<GetCountriesResponce>>
    {
        private readonly ICountryRepository _countryRepository;

        public GetFiltredCountriesHandler(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        public async Task<PagedResult<GetCountriesResponce>> Handle(GetFiltredCountriesQuery request, CancellationToken cancellationToken)
        {

            var agencyTiers = await _countryRepository.GetPagedByCriteriaAsync(request,
               request.PageNumber,
               request.PageSize,
               cancellationToken,
               c => c.Currency);

            return new PagedResult<GetCountriesResponce>(agencyTiers.Items.Adapt<List<GetCountriesResponce>>(),
                agencyTiers.TotalCount,
                agencyTiers.PageNumber,
                agencyTiers.PageSize);
        }
    }
}
