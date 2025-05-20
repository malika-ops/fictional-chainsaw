using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.SupportAccounts.Queries.GetAllSupportAccounts;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class SupportAccountRepository : ISupportAccountRepository
{
    private readonly ApplicationDbContext _context;

    public SupportAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupportAccount>> GetAllSupportAccountsAsync(CancellationToken cancellationToken)
    {
        return await _context.SupportAccounts.ToListAsync(cancellationToken);
    }

    public IQueryable<SupportAccount> GetAllSupportAccountsQueryable(CancellationToken cancellationToken)
    {
        return _context.SupportAccounts
            .Include(s => s.Partner)
            .AsNoTracking();
    }

    public async Task<SupportAccount?> GetByIdAsync(SupportAccountId id, CancellationToken cancellationToken)
    {
        return await _context.SupportAccounts
            .Where(s => s.Id == id)
            .Include(s => s.Partner)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SupportAccount?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _context.SupportAccounts
            .Where(s => s.Code.ToLower() == code.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SupportAccount?> GetByAccountingNumberAsync(string accountingNumber, CancellationToken cancellationToken)
    {
        return await _context.SupportAccounts
            .Where(s => s.AccountingNumber.ToLower() == accountingNumber.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SupportAccount> AddSupportAccountAsync(SupportAccount supportAccount, CancellationToken cancellationToken)
    {
        await _context.SupportAccounts.AddAsync(supportAccount, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return supportAccount;
    }

    public async Task UpdateSupportAccountAsync(SupportAccount supportAccount, CancellationToken cancellationToken)
    {
        _context.SupportAccounts.Update(supportAccount);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSupportAccountAsync(SupportAccount supportAccount, CancellationToken cancellationToken)
    {
        _context.SupportAccounts.Remove(supportAccount);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<SupportAccount>> GetFilteredSupportAccountsAsync(GetAllSupportAccountsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.SupportAccounts
            .Include(s => s.Partner)
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllSupportAccountsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.SupportAccounts
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<List<SupportAccount>> GetByPartnerIdAsync(Guid partnerId, CancellationToken cancellationToken)
    {
        return await _context.SupportAccounts
            .Where(s => s.PartnerId.Value == partnerId)
            .Include(s => s.Partner)
            .ToListAsync(cancellationToken);
    }

    private List<Expression<Func<SupportAccount, bool>>> BuildFilters(GetAllSupportAccountsQuery request)
    {
        var filters = new List<Expression<Func<SupportAccount, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            filters.Add(s => s.Code.ToUpper().Equals(request.Code.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            filters.Add(s => s.Name.ToUpper().Contains(request.Name.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.AccountingNumber))
        {
            filters.Add(s => s.AccountingNumber.ToUpper().Equals(request.AccountingNumber.ToUpper()));
        }

        if (request.MinThreshold.HasValue)
        {
            filters.Add(s => s.Threshold >= request.MinThreshold.Value);
        }

        if (request.MaxThreshold.HasValue)
        {
            filters.Add(s => s.Threshold <= request.MaxThreshold.Value);
        }

        if (request.MinLimit.HasValue)
        {
            filters.Add(s => s.Limit >= request.MinLimit.Value);
        }

        if (request.MaxLimit.HasValue)
        {
            filters.Add(s => s.Limit <= request.MaxLimit.Value);
        }

        if (request.MinAccountBalance.HasValue)
        {
            filters.Add(s => s.AccountBalance >= request.MinAccountBalance.Value);
        }

        if (request.MaxAccountBalance.HasValue)
        {
            filters.Add(s => s.AccountBalance <= request.MaxAccountBalance.Value);
        }

        if (request.PartnerId.HasValue && request.PartnerId != Guid.Empty)
        {
            filters.Add(s => s.PartnerId.Value == request.PartnerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SupportAccountType))
        {
            if (Enum.TryParse<SupportAccountType>(request.SupportAccountType, true, out var supportAccountTypeEnum))
            {
                filters.Add(s => s.SupportAccountType == supportAccountTypeEnum);
            }
            else
            {
                filters.Add(s => false);
            }
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(s => s.IsEnabled == request.IsEnabled.Value);
        }

        return filters;
    }
}