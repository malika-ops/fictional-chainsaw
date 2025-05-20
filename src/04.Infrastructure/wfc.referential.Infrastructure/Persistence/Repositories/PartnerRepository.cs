using System.Linq.Expressions;
using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Partners.Queries.GetAllPartners;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Infrastructure.Data.Repositories;

public class PartnerRepository : IPartnerRepository
{
    private readonly ApplicationDbContext _context;

    public PartnerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Partner>> GetAllPartnersAsync(CancellationToken cancellationToken)
    {
        return await _context.Partners.ToListAsync(cancellationToken);
    }

    public IQueryable<Partner> GetAllPartnersQueryable(CancellationToken cancellationToken)
    {
        return _context.Partners
            .Include(p => p.CommissionAccount)
            .Include(p => p.ActivityAccount)
            .Include(p => p.SupportAccount)
            .AsNoTracking();
    }

    public async Task<Partner?> GetByIdAsync(PartnerId id, CancellationToken cancellationToken)
    {
        return await _context.Partners
            .Where(p => p.Id == id)
            .Include(p => p.CommissionAccount)
            .Include(p => p.ActivityAccount)
            .Include(p => p.SupportAccount)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Partner?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _context.Partners
            .Where(p => p.Code.ToLower() == code.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Partner?> GetByIdentificationNumberAsync(string identificationNumber, CancellationToken cancellationToken)
    {
        return await _context.Partners
            .Where(p => p.TaxIdentificationNumber.ToLower() == identificationNumber.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Partner?> GetByICEAsync(string ice, CancellationToken cancellationToken)
    {
        return await _context.Partners
            .Where(p => p.ICE.ToLower() == ice.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Partner> AddPartnerAsync(Partner partner, CancellationToken cancellationToken)
    {
        await _context.Partners.AddAsync(partner, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return partner;
    }

    public async Task UpdatePartnerAsync(Partner partner, CancellationToken cancellationToken)
    {
        _context.Partners.Update(partner);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePartnerAsync(Partner partner, CancellationToken cancellationToken)
    {
        _context.Partners.Remove(partner);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Partner>> GetFilteredPartnersAsync(GetAllPartnersQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Partners
            .Include(p => p.CommissionAccount)
            .Include(p => p.ActivityAccount)
            .Include(p => p.SupportAccount)
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountTotalAsync(GetAllPartnersQuery request, CancellationToken cancellationToken)
    {
        var filters = BuildFilters(request);

        var query = _context.Partners
            .AsNoTracking()
            .ApplyFilters(filters);

        return await query.CountAsync(cancellationToken);
    }

    private List<Expression<Func<Partner, bool>>> BuildFilters(GetAllPartnersQuery request)
    {
        var filters = new List<Expression<Func<Partner, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            filters.Add(p => p.Code.ToUpper().Equals(request.Code.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.Label))
        {
            filters.Add(p => p.Label.ToUpper().Contains(request.Label.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            filters.Add(p => p.Type.ToUpper().Contains(request.Type.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.NetworkMode))
        {
            if (Enum.TryParse<NetworkMode>(request.NetworkMode, true, out var networkModeEnum))
            {
                filters.Add(p => p.NetworkMode == networkModeEnum);
            }
            else
            {
                filters.Add(p => false);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentMode))
        {
            if (Enum.TryParse<PaymentMode>(request.PaymentMode, true, out var paymentModeEnum))
            {
                filters.Add(p => p.PaymentMode == paymentModeEnum);
            }
            else
            {
                filters.Add(p => false);
            }
        }

        if (request.IdParent.HasValue)
        {
            filters.Add(p => p.IdParent == request.IdParent);
        }

        if (!string.IsNullOrWhiteSpace(request.SupportAccountType))
        {
            if (Enum.TryParse<Domain.SupportAccountAggregate.SupportAccountType>(request.SupportAccountType, true, out var supportAccountTypeEnum))
            {
                filters.Add(p => p.SupportAccountType == supportAccountTypeEnum);
            }
            else
            {
                filters.Add(p => false);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.TaxIdentificationNumber))
        {
            filters.Add(p => p.TaxIdentificationNumber.ToUpper().Equals(request.TaxIdentificationNumber.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(request.ICE))
        {
            filters.Add(p => p.ICE.ToUpper().Equals(request.ICE.ToUpper()));
        }

        if (request.CommissionAccountId.HasValue)
        {
            filters.Add(p => p.CommissionAccountId == request.CommissionAccountId);
        }

        if (request.ActivityAccountId.HasValue)
        {
            filters.Add(p => p.ActivityAccountId == request.ActivityAccountId);
        }

        if (request.SupportAccountId.HasValue)
        {
            filters.Add(p => p.SupportAccountId == request.SupportAccountId);
        }

        if (request.IsEnabled.HasValue)
        {
            filters.Add(p => p.IsEnabled == request.IsEnabled.Value);
        }

        return filters;
    }
}