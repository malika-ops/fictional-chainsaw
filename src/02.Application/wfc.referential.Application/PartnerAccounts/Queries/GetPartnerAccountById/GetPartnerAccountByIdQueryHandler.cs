using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.PartnerAccounts.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Queries.GetPartnerAccountById;

public class GetPartnerAccountByIdQueryHandler : IQueryHandler<GetPartnerAccountByIdQuery, PartnerAccountResponse>
{
    private readonly IPartnerAccountRepository _partnerAccountRepository;

    public GetPartnerAccountByIdQueryHandler(IPartnerAccountRepository partnerAccountRepository)
    {
        _partnerAccountRepository = partnerAccountRepository;
    }

    public async Task<PartnerAccountResponse> Handle(GetPartnerAccountByIdQuery query, CancellationToken ct)
    {
        var id = PartnerAccountId.Of(query.PartnerAccountId);
        var entity = await _partnerAccountRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"PartnerAccount with id '{query.PartnerAccountId}' not found.");

        return entity.Adapt<PartnerAccountResponse>();
    }
} 