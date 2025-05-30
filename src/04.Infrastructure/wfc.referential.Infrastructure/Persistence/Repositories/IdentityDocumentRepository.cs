using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;
using wfc.referential.Infrastructure.Data;

namespace wfc.referential.Infrastructure.Persistence.Repositories;

public class IdentityDocumentRepository : BaseRepository<IdentityDocument, IdentityDocumentId>, IIdentityDocumentRepository
{
    public IdentityDocumentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}