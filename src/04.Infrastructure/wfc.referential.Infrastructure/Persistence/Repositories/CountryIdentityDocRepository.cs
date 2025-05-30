using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;
using wfc.referential.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class CountryIdentityDocRepository : BaseRepository<CountryIdentityDoc, CountryIdentityDocId>, ICountryIdentityDocRepository
{
    private readonly ApplicationDbContext _context;

    public CountryIdentityDocRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByCountryAndIdentityDocumentAsync(CountryId countryId, IdentityDocumentId identityDocumentId, CancellationToken cancellationToken = default)
    {
        return await _context.CountryIdentityDocs
            .AnyAsync(x => x.CountryId == countryId && x.IdentityDocumentId == identityDocumentId, cancellationToken);
    }
}