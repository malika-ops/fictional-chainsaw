using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Partners.Queries.GetAllPartners;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IPartnerRepository
{
    Task<List<Partner>> GetAllPartnersAsync(CancellationToken cancellationToken);

    IQueryable<Partner> GetAllPartnersQueryable(CancellationToken cancellationToken);

    Task<Partner?> GetByIdAsync(PartnerId id, CancellationToken cancellationToken);

    Task<Partner?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    Task<Partner?> GetByIdentificationNumberAsync(string identificationNumber, CancellationToken cancellationToken);

    Task<Partner?> GetByICEAsync(string ice, CancellationToken cancellationToken);

    Task<Partner> AddPartnerAsync(Partner partner, CancellationToken cancellationToken);

    Task UpdatePartnerAsync(Partner partner, CancellationToken cancellationToken);

    Task DeletePartnerAsync(Partner partner, CancellationToken cancellationToken);

    Task<List<Partner>> GetFilteredPartnersAsync(GetAllPartnersQuery request, CancellationToken cancellationToken);

    Task<int> GetCountTotalAsync(GetAllPartnersQuery request, CancellationToken cancellationToken);
}