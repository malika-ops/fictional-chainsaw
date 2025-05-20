using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Taxes.Queries.GetAllTaxes;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class TaxRepository : ITaxRepository
{
    private readonly ApplicationDbContext _context;

    public TaxRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<Tax>> GetAllTaxesAsync(CancellationToken cancellationToken)
    {
        return await _context.Taxes.ToListAsync(cancellationToken);
    }
    public async Task<Tax?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Taxes
            .Where(Tax => Tax.Id == TaxId.Of(id))
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<Tax?> GetByCodeAsync(string TaxCode, CancellationToken cancellationToken)
    {
        return await _context.Taxes
            .Where(Tax => Tax.Code == TaxCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Tax> AddTaxAsync(Tax tax, CancellationToken cancellationToken)
    {
        await _context.Taxes.AddAsync(tax);
        await _context.SaveChangesAsync(cancellationToken);

        return tax;
    }

    public async Task UpdateTaxAsync(Tax tax, CancellationToken cancellationToken)
    {
        _context.Taxes.Update(tax);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTaxAsync(Tax tax, CancellationToken cancellationToken)
    {

        _context.Taxes.Remove(tax);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Tax>> GetTaxesByCriteriaAsync(GetAllTaxesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Taxes
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllTaxesQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Taxes
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private List<Expression<Func<Tax, bool>>> BuildFilters(GetAllTaxesQuery request)
{
    var filters = new List<Expression<Func<Tax, bool>>>();

    if (!string.IsNullOrWhiteSpace(request.Code))
    {
        filters.Add(tax => tax.Code.Equals(request.Code));
    }

    if (request.Description is not null)
    {
        filters.Add(tax => tax.Description.Contains(request.Description, StringComparison.OrdinalIgnoreCase));
    }

    if (request.FixedAmount.HasValue)
    {
        filters.Add(tax =>tax.FixedAmount.Equals(request.FixedAmount));
    }
    filters.Add(tax => tax.IsEnabled.Equals(request.IsEnabled));
    return filters;
}

}
