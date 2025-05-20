using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.PartnerAccounts.Queries.GetAllPartnerAccounts;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class PartnerAccountRepository : IPartnerAccountRepository
{
    private readonly ApplicationDbContext _context;

    public PartnerAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PartnerAccount>> GetAllPartnerAccountsAsync(CancellationToken cancellationToken)
    {
        return await _context.PartnerAccounts
            .Include(p => p.Bank)
            .Include(p => p.AccountType)
            .ToListAsync(cancellationToken);
    }

    public IQueryable<PartnerAccount> GetAllPartnerAccountsQueryable(CancellationToken cancellationToken)
    {
        return _context.PartnerAccounts
            .Include(p => p.Bank)
            .Include(p => p.AccountType)
            .AsNoTracking();
    }

    public async Task<PartnerAccount?> GetByIdAsync(PartnerAccountId id, CancellationToken cancellationToken)
    {
        return await _context.PartnerAccounts
            .Where(p => p.Id == id)
            .Include(p => p.Bank)
            .Include(p => p.AccountType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PartnerAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken)
    {
        return await _context.PartnerAccounts
            .Where(p => p.AccountNumber.ToLower() == accountNumber.ToLower())
            .Include(p => p.Bank)
            .Include(p => p.AccountType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PartnerAccount?> GetByRIBAsync(string rib, CancellationToken cancellationToken)
    {
        return await _context.PartnerAccounts
            .Where(p => p.RIB.ToLower() == rib.ToLower())
            .Include(p => p.Bank)
            .Include(p => p.AccountType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PartnerAccount> AddPartnerAccountAsync(PartnerAccount partnerAccount, CancellationToken cancellationToken)
    {
        await _context.PartnerAccounts.AddAsync(partnerAccount, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return partnerAccount;
    }

    public async Task UpdatePartnerAccountAsync(PartnerAccount partnerAccount, CancellationToken cancellationToken)
    {
        _context.PartnerAccounts.Update(partnerAccount);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePartnerAccountAsync(PartnerAccount partnerAccount, CancellationToken cancellationToken)
    {
        _context.PartnerAccounts.Remove(partnerAccount);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<PartnerAccount>> GetFilteredPartnerAccountsAsync(GetAllPartnerAccountsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.PartnerAccounts
            .Include(p => p.Bank)
            .Include(p => p.AccountType)
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllPartnerAccountsQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.PartnerAccounts
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<List<PartnerAccount>> GetByBankIdAsync(Guid bankId, CancellationToken cancellationToken)
    {
        return await _context.PartnerAccounts
            .Where(p => p.BankId.Value == bankId)
            .Include(p => p.Bank)
            .Include(p => p.AccountType)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PartnerAccount>> GetByAccountTypeIdAsync(Guid accountTypeId, CancellationToken cancellationToken)
    {
        return await _context.PartnerAccounts
            .Where(p => p.AccountTypeId.Value == accountTypeId)
            .Include(p => p.Bank)
            .Include(p => p.AccountType)
            .ToListAsync(cancellationToken);
    }

    private List<Expression<Func<PartnerAccount, bool>>> BuildFilters(GetAllPartnerAccountsQuery request)
    {
        var filters = new List<Expression<Func<PartnerAccount, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            filters.Add(p => p.AccountNumber.ToUpper().Equals(request.AccountNumber.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.RIB))
        {
            filters.Add(p => p.RIB.ToUpper().Equals(request.RIB.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.BusinessName))
        {
            filters.Add(p => p.BusinessName != null && p.BusinessName.ToUpper().Contains(request.BusinessName.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.ShortName))
        {
            filters.Add(p => p.ShortName != null && p.ShortName.ToUpper().Contains(request.ShortName.ToUpper()));
        }

        if (request.BankId.HasValue && request.BankId != Guid.Empty)
        {
            filters.Add(p => p.BankId.Value == request.BankId.Value);
        }

        if (request.AccountTypeId.HasValue && request.AccountTypeId != Guid.Empty)
        {
            filters.Add(p => p.AccountTypeId.Value == request.AccountTypeId.Value);
        }

        if (request.MinAccountBalance.HasValue)
        {
            filters.Add(p => p.AccountBalance >= request.MinAccountBalance.Value);
        }

        if (request.MaxAccountBalance.HasValue)
        {
            filters.Add(p => p.AccountBalance <= request.MaxAccountBalance.Value);
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(p => p.IsEnabled == request.IsEnabled.Value);
        }

        return filters;
    }
}