using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.CountryIdentityDocs.Queries.GetAllCountryIdentityDocs;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class CountryIdentityDocRepository : ICountryIdentityDocRepository
{
    private readonly ApplicationDbContext _context;

    public CountryIdentityDocRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CountryIdentityDoc>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.CountryIdentityDocs.ToListAsync(cancellationToken);
    }

    public async Task<CountryIdentityDoc?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.CountryIdentityDocs
            .FirstOrDefaultAsync(x => x.Id == new CountryIdentityDocId(id), cancellationToken);
    }

    public async Task<bool> ExistsByCountryAndIdentityDocumentAsync(CountryId countryId, IdentityDocumentId identityDocumentId, CancellationToken cancellationToken = default)
    {
        return await _context.CountryIdentityDocs
            .AnyAsync(x => x.CountryId == countryId && x.IdentityDocumentId == identityDocumentId, cancellationToken);
    }

    public async Task<IEnumerable<CountryIdentityDoc>> GetByCountryIdAsync(CountryId countryId, CancellationToken cancellationToken = default)
    {
        return await _context.CountryIdentityDocs
            .Where(x => x.CountryId == countryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CountryIdentityDoc>> GetByIdentityDocumentIdAsync(IdentityDocumentId identityDocumentId, CancellationToken cancellationToken = default)
    {
        return await _context.CountryIdentityDocs
            .Where(x => x.IdentityDocumentId == identityDocumentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<CountryIdentityDoc> AddAsync(CountryIdentityDoc entity, CancellationToken cancellationToken)
    {
        await _context.CountryIdentityDocs.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(CountryIdentityDoc entity, CancellationToken cancellationToken)
    {
        _context.CountryIdentityDocs.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CountryIdentityDoc entity, CancellationToken cancellationToken)
    {
        _context.CountryIdentityDocs.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<List<CountryIdentityDoc>> GetFilteredAsync(GetAllCountryIdentityDocsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.CountryIdentityDocs
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllCountryIdentityDocsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.CountryIdentityDocs
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private List<Expression<Func<CountryIdentityDoc, bool>>> BuildFilters(GetAllCountryIdentityDocsQuery request)
    {
        var filters = new List<Expression<Func<CountryIdentityDoc, bool>>>();

        if (request.CountryId.HasValue)
        {
            var countryId = new CountryId(request.CountryId.Value);
            filters.Add(cid => cid.CountryId == countryId);
        }

        if (request.IdentityDocumentId.HasValue)
        {
            var identityDocumentId = IdentityDocumentId.Of(request.IdentityDocumentId.Value);
            filters.Add(cid => cid.IdentityDocumentId == identityDocumentId);
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(cid => cid.IsEnabled == request.IsEnabled.Value);
        }

        return filters;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) 
    { 
        return await _context.SaveChangesAsync(); 
    }
}