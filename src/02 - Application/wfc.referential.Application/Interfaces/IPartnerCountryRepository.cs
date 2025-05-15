using wfc.referential.Application.PartnerCountries.Queries.GetAllPartnerCountries;
using wfc.referential.Domain.PartnerCountryAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IPartnerCountryRepository
{
    Task<PartnerCountry?> GetByPartnerAndCountryAsync(Guid partnerId, Guid countryId, CancellationToken ct);
    Task<PartnerCountry> AddAsync(PartnerCountry entity, CancellationToken ct);
    Task<PartnerCountry?> GetByIdAsync(Guid id, CancellationToken ct);
    Task UpdateAsync(PartnerCountry entity, CancellationToken ct);
    Task<List<PartnerCountry>> GetAllPaginatedAsyncFiltered(GetAllPartnerCountriesQuery q, CancellationToken ct);
    Task<int> GetTotalCountAsync(GetAllPartnerCountriesQuery q, CancellationToken ct);

}
