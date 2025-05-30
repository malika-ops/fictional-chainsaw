using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.Interfaces;

public interface ICountryIdentityDocRepository : IRepositoryBase<CountryIdentityDoc, CountryIdentityDocId>
{
    Task<bool> ExistsByCountryAndIdentityDocumentAsync(CountryId countryId, IdentityDocumentId identityDocumentId, CancellationToken cancellationToken = default);
}