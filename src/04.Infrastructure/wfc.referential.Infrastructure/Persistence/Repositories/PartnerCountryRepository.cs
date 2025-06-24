using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerCountryAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class PartnerCountryRepository : BaseRepository<PartnerCountry, PartnerCountryId>, IPartnerCountryRepository
{
    public PartnerCountryRepository(ApplicationDbContext context) : base(context) { }
}