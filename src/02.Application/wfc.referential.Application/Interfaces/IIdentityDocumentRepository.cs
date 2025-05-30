using BuildingBlocks.Core.Abstraction.Repositories;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IIdentityDocumentRepository : IRepositoryBase<IdentityDocument, IdentityDocumentId>
{
}