using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;

public class GetAllIdentityDocumentsHandler
    : IQueryHandler<GetAllIdentityDocumentsQuery, PagedResult<GetIdentityDocumentsResponse>>
{
    private readonly IIdentityDocumentRepository _repo;

    public GetAllIdentityDocumentsHandler(IIdentityDocumentRepository repo) => _repo = repo;

    public async Task<PagedResult<GetIdentityDocumentsResponse>> Handle(
        GetAllIdentityDocumentsQuery identityDocumentQuery, CancellationToken ct)
    {
        var identityDocuments = await _repo.GetPagedByCriteriaAsync(identityDocumentQuery, identityDocumentQuery.PageNumber, identityDocumentQuery.PageSize, ct);
        return new PagedResult<GetIdentityDocumentsResponse>(identityDocuments.Items.Adapt<List<GetIdentityDocumentsResponse>>(), identityDocuments.TotalCount, identityDocuments.PageNumber, identityDocuments.PageSize);
    }
}