using wfc.referential.Application.CountryIdentityDocs.Queries.GetAllCountryIdentityDocs;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICountryIdentityDocRepository
{
    Task<List<CountryIdentityDoc>> GetAllAsync(CancellationToken cancellationToken);
    Task<CountryIdentityDoc?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByCountryAndIdentityDocumentAsync(CountryId countryId, IdentityDocumentId identityDocumentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CountryIdentityDoc>> GetByCountryIdAsync(CountryId countryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CountryIdentityDoc>> GetByIdentityDocumentIdAsync(IdentityDocumentId identityDocumentId, CancellationToken cancellationToken = default);
    Task<CountryIdentityDoc> AddAsync(CountryIdentityDoc entity, CancellationToken cancellationToken);
    Task UpdateAsync(CountryIdentityDoc entity, CancellationToken cancellationToken);
    Task DeleteAsync(CountryIdentityDoc entity, CancellationToken cancellationToken);
    Task<List<CountryIdentityDoc>> GetFilteredAsync(GetAllCountryIdentityDocsQuery request, CancellationToken cancellationToken);
    Task<int> GetCountTotalAsync(GetAllCountryIdentityDocsQuery request, CancellationToken cancellationToken);
}