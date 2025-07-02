using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Queries.GetIdentityDocumentById;

public class GetIdentityDocumentByIdQueryHandler : IQueryHandler<GetIdentityDocumentByIdQuery, GetIdentityDocumentsResponse>
{
    private readonly IIdentityDocumentRepository _identityDocumentRepository;

    public GetIdentityDocumentByIdQueryHandler(IIdentityDocumentRepository identityDocumentRepository)
    {
        _identityDocumentRepository = identityDocumentRepository;
    }

    public async Task<GetIdentityDocumentsResponse> Handle(GetIdentityDocumentByIdQuery query, CancellationToken ct)
    {
        var id = IdentityDocumentId.Of(query.IdentityDocumentId);
        var entity = await _identityDocumentRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"IdentityDocument with id '{query.IdentityDocumentId}' not found.");

        return entity.Adapt<GetIdentityDocumentsResponse>();
    }
} 