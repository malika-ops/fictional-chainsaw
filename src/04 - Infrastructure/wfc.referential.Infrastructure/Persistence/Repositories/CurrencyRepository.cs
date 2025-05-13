using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Currencies.Queries;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly ApplicationDbContext _context;

    public CurrencyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Currency>> GetAllCurrenciesAsync(CancellationToken cancellationToken)
    {
        return await _context.Currencies.ToListAsync(cancellationToken);
    }

    public IQueryable<Currency> GetAllCurrenciesQueryable(CancellationToken cancellationToken)
    {
        return _context.Currencies
            .Include(c => c.Countries)
            .AsNoTracking();
    }

    public async Task<Currency?> GetByIdAsync(CurrencyId id, CancellationToken cancellationToken)
    {
        return await _context.Currencies
            .Where(c => c.Id == id)
            .Include(c => c.Countries)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _context.Currencies
            .Where(c => c.Code == code)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Currency?> GetByCodeIsoAsync(int codeiso, CancellationToken cancellationToken)
    {
        return await _context.Currencies
            .Where(c => c.CodeIso == codeiso)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Currency> AddCurrencyAsync(Currency currency, CancellationToken cancellationToken)
    {
        await _context.Currencies.AddAsync(currency, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return currency;
    }

    public async Task UpdateCurrencyAsync(Currency currency, CancellationToken cancellationToken)
    {
        _context.Currencies.Update(currency);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCurrencyAsync(Currency currency, CancellationToken cancellationToken)
    {
        _context.Currencies.Remove(currency);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsCurrencyAssociatedWithCountryAsync(CurrencyId id, CancellationToken cancellationToken)
    {
        return await _context.Countries
            .AnyAsync(c => c.CurrencyId == id, cancellationToken);
    }

    public async Task<List<Currency>> GetCurrenciesByCriteriaAsync(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);
        var query = _context.Currencies
            .Include(c => c.Countries)
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);
        var query = _context.Currencies
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private List<Expression<Func<Currency, bool>>> BuildFilters(GetAllCurrenciesQuery request)
    {
        var filters = new List<Expression<Func<Currency, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            filters.Add(c => c.Code.ToUpper().Equals(request.Code.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.CodeAR))
        {
            filters.Add(c => c.CodeAR.ToUpper().Contains(request.CodeAR.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.CodeEN))
        {
            filters.Add(c => c.CodeEN.ToUpper().Contains(request.CodeEN.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            filters.Add(c => c.Name.ToUpper().Equals(request.Name.ToUpper()));
        }

        if (request.CodeIso.HasValue)
        {
            filters.Add(c => c.CodeIso == request.CodeIso.Value);
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(c => c.IsEnabled == request.IsEnabled.Value);
        }

        return filters;
    }
}