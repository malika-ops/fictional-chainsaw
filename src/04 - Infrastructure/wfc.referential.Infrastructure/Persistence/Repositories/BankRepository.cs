using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Banks.Queries.GetAllBanks;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class BankRepository : IBankRepository
{
    private readonly ApplicationDbContext _context;

    public BankRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Bank>> GetAllBanksAsync(CancellationToken cancellationToken)
    {
        return await _context.Banks.ToListAsync(cancellationToken);
    }

    public IQueryable<Bank> GetAllBanksQueryable(CancellationToken cancellationToken)
    {
        return _context.Banks.AsNoTracking();
    }

    public async Task<Bank?> GetByIdAsync(BankId id, CancellationToken cancellationToken)
    {
        return await _context.Banks
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Bank?> GetByCodeAsync(string bankCode, CancellationToken cancellationToken)
    {
        return await _context.Banks
            .Where(b => b.Code.ToLower() == bankCode.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Bank> AddBankAsync(Bank bank, CancellationToken cancellationToken)
    {
        await _context.Banks.AddAsync(bank, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return bank;
    }

    public async Task UpdateBankAsync(Bank bank, CancellationToken cancellationToken)
    {
        _context.Banks.Update(bank);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteBankAsync(Bank bank, CancellationToken cancellationToken)
    {
        _context.Banks.Remove(bank);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Bank>> GetFilteredBanksAsync(GetAllBanksQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Banks
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllBanksQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Banks
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> HasLinkedAccountsAsync(BankId bankId, CancellationToken cancellationToken)
    {
        return await _context.PartnerAccounts
            .AnyAsync(a => a.BankId == bankId, cancellationToken);
    }

    private List<Expression<Func<Bank, bool>>> BuildFilters(GetAllBanksQuery request)
    {
        var filters = new List<Expression<Func<Bank, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            filters.Add(b => b.Code.ToUpper().Equals(request.Code.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            filters.Add(b => b.Name.ToUpper().Equals(request.Name.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.Abbreviation))
        {
            filters.Add(b => b.Abbreviation.ToUpper().Equals(request.Abbreviation.ToUpper()));
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(b => b.IsEnabled == request.IsEnabled.Value);
        }

        return filters;
    }
}