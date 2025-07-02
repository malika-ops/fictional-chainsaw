using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.SupportAccounts.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.SupportAccounts.Queries.GetSupportAccountById;

public class GetSupportAccountByIdQueryHandler : IQueryHandler<GetSupportAccountByIdQuery, GetSupportAccountsResponse>
{
    private readonly ISupportAccountRepository _supportAccountRepository;

    public GetSupportAccountByIdQueryHandler(ISupportAccountRepository supportAccountRepository)
    {
        _supportAccountRepository = supportAccountRepository;
    }

    public async Task<GetSupportAccountsResponse> Handle(GetSupportAccountByIdQuery query, CancellationToken ct)
    {
        var id = SupportAccountId.Of(query.SupportAccountId);
        var entity = await _supportAccountRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"SupportAccount with id '{query.SupportAccountId}' not found.");

        return entity.Adapt<GetSupportAccountsResponse>();
    }
} 