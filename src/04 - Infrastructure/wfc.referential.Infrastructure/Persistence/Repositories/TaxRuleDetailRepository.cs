using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.TaxRuleDetails.Queries.GetAllTaxeRuleDetails;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class TaxRuleDetailRepository : ITaxRuleDetailRepository
{
    private readonly ApplicationDbContext _context;

    public TaxRuleDetailRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaxRuleDetail>> GetAllTaxRuleDetailsAsync(CancellationToken cancellationToken)
    {
        return await _context.TaxRuleDetails
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<TaxRuleDetail?> GetTaxRuleDetailByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.TaxRuleDetails
            .Where(trd => trd.Id == TaxRuleDetailsId.Of(id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TaxRuleDetail> AddTaxRuleDetailAsync(TaxRuleDetail taxRuleDetail, CancellationToken cancellationToken)
    {
        await _context.TaxRuleDetails.AddAsync(taxRuleDetail, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return taxRuleDetail;
    }

    public async Task UpdateTaxRuleDetailAsync(TaxRuleDetail taxRuleDetail, CancellationToken cancellationToken)
    {
        _context.TaxRuleDetails.Update(taxRuleDetail);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTaxRuleDetailAsync(TaxRuleDetail taxRuleDetail, CancellationToken cancellationToken)
    {
        _context.TaxRuleDetails.Remove(taxRuleDetail);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TaxRuleDetail>> GetTaxRuleDetailsByCriteriaAsync(GetAllTaxRuleDetailsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.TaxRuleDetails
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllTaxRuleDetailsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.TaxRuleDetails
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<TaxRuleDetail?> GetByCorridorTaxServiceAsync(CorridorId corridorId, TaxId taxId, ServiceId serviceId, CancellationToken cancellationToken)
    {
        return await _context.TaxRuleDetails
            .Where(trd => trd.CorridorId == corridorId && trd.TaxId == taxId && trd.ServiceId == serviceId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private List<Expression<Func<TaxRuleDetail, bool>>> BuildFilters(GetAllTaxRuleDetailsQuery request)
    {
        var filters = new List<Expression<Func<TaxRuleDetail, bool>>>();

        if (request.CorridorId is not null)
        {
            filters.Add(trd => trd.CorridorId == request.CorridorId);
        }

        if (request.TaxId is not null)
        {
            filters.Add(trd => trd.TaxId == request.TaxId);
        }

        if (request.ServiceId is not null)
        {
            filters.Add(trd => trd.ServiceId == request.ServiceId);
        }

        if (request.AppliedOn.HasValue)
        {
            filters.Add(trd => string.Equals(trd.AppliedOn, request.AppliedOn));
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(trd => trd.IsEnabled == request.IsEnabled);
        }

        return filters;
    }
}