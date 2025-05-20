using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using wfc.referential.Application.Agencies.Queries.GetAllAgencies;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class AgencyRepository : IAgencyRepository
{
    private readonly ApplicationDbContext _context;

    public AgencyRepository(ApplicationDbContext context) => _context = context;

    public async Task<Agency?> GetByCodeAsync(string code, CancellationToken ct)
    {
        return await _context.Agencies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Code.ToLower() == code.ToLower(), ct);
    }

    public async Task<Agency> AddAsync(Agency agency, CancellationToken ct)
    {
        await _context.Agencies.AddAsync(agency, ct);
        await _context.SaveChangesAsync(ct);
        return agency;
    }

    public async Task<Agency?> GetByIdAsync(Guid id, CancellationToken ct) =>
    await _context.Agencies.FirstOrDefaultAsync(a => a.Id == new AgencyId(id), ct);

    public async Task UpdateAsync(Agency agency, CancellationToken ct)
    {
        _context.Agencies.Update(agency);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<Agency>> GetAllAgenciesPaginatedAsyncFiltered(
       GetAllAgenciesQuery q, CancellationToken ct)
    {
        var filters = BuildFilters(q);

        var query = _context.Agencies
                            .Include(a => a.AgencyType)                // <-- add
                            .ThenInclude(pt => pt.TypeDefinition)      // <-- add
                            .AsNoTracking()
                            .ApplyFilters(filters);

        return await query
            .Skip((q.PageNumber - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountTotalAsync(
        GetAllAgenciesQuery q, CancellationToken ct)
    {
        var filters = BuildFilters(q);

        return await _context.Agencies
            .Include(a => a.AgencyType)
            .ThenInclude(pt => pt.TypeDefinition)
            .AsNoTracking()
            .ApplyFilters(filters)
            .CountAsync(ct);
    }


    private static List<Expression<Func<Agency, bool>>> BuildFilters(GetAllAgenciesQuery q)
    {
        var f = new List<Expression<Func<Agency, bool>>>();

        if (!string.IsNullOrWhiteSpace(q.Code)) f.Add(a => a.Code.ToUpper().Equals(q.Code.ToUpper()));
        if (!string.IsNullOrWhiteSpace(q.Name)) f.Add(a => a.Name.ToUpper().Equals(q.Name.ToUpper()));
        if (!string.IsNullOrWhiteSpace(q.Abbreviation)) f.Add(a => a.Abbreviation.ToUpper().Equals(q.Abbreviation.ToUpper()));
        if (!string.IsNullOrWhiteSpace(q.Address)) f.Add(a => a.Address1.ToUpper().Equals(q.Address.ToUpper()) || (a.Address2 != null && a.Address2.ToUpper().Equals(q.Address.ToUpper())));
        if (!string.IsNullOrWhiteSpace(q.Phone)) f.Add(a => a.Phone.ToUpper().Equals(q.Phone.ToUpper()));
        if (!string.IsNullOrWhiteSpace(q.Fax)) f.Add(a => a.Fax.ToUpper().Equals(q.Fax.ToUpper()));
        if (!string.IsNullOrWhiteSpace(q.AccountingSheetName)) f.Add(a => a.AccountingSheetName.ToUpper().Equals(q.AccountingSheetName.ToUpper()));
        if (!string.IsNullOrWhiteSpace(q.AccountingAccountNumber)) f.Add(a => a.AccountingAccountNumber.ToUpper().Equals(q.AccountingAccountNumber.ToUpper()));
        if (!string.IsNullOrWhiteSpace(q.MoneyGramReferenceNumber)) f.Add(a => a.MoneyGramReferenceNumber.ToUpper().Equals(q.MoneyGramReferenceNumber.ToUpper()));
        if (!string.IsNullOrWhiteSpace(q.PostalCode)) f.Add(a => a.PostalCode.ToUpper().Equals(q.PostalCode.ToUpper()));

        if (q.CityId.HasValue) f.Add(a => a.CityId == new CityId(q.CityId.Value));
        if (q.SectorId.HasValue) f.Add(a => a.SectorId == new SectorId(q.SectorId.Value));
        if (q.AgencyTypeId.HasValue) f.Add(a => a.AgencyTypeId == new ParamTypeId(q.AgencyTypeId.Value));

        if (!string.IsNullOrWhiteSpace(q.AgencyTypeValue))
            f.Add(a => a.AgencyType != null && a.AgencyType.Value.ToUpper().Equals(q.AgencyTypeValue.ToUpper()));

        if (!string.IsNullOrWhiteSpace(q.AgencyTypeLibelle))
            f.Add(a => a.AgencyType != null && a.AgencyType.TypeDefinition.Libelle.ToUpper().Equals(q.AgencyTypeLibelle.ToUpper()));

        if (q.IsEnabled.HasValue) f.Add(a => a.IsEnabled == q.IsEnabled.Value);

        return f;
    }
}