using wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.Interfaces;

public interface IIdentityDocumentRepository
{
    Task<List<IdentityDocument>> GetAllAsync(CancellationToken cancellationToken);
    Task<IdentityDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IdentityDocument?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<IdentityDocument> AddAsync(IdentityDocument entity, CancellationToken cancellationToken);
    Task UpdateAsync(IdentityDocument entity, CancellationToken cancellationToken);
    Task DeleteAsync(IdentityDocument entity, CancellationToken cancellationToken);
    Task<List<IdentityDocument>> GetByCriteriaAsync(GetAllIdentityDocumentsQuery request, CancellationToken cancellationToken);
    Task<int> GetCountAsync(GetAllIdentityDocumentsQuery request, CancellationToken cancellationToken);
}