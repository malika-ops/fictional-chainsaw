using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;

namespace wfc.referential.Application.Banks.Queries.GetBankById;

public class GetBankByIdQueryHandler : IQueryHandler<GetBankByIdQuery, GetBanksResponse>
{
    private readonly IBankRepository _bankRepository;

    public GetBankByIdQueryHandler(IBankRepository bankRepository)
    {
        _bankRepository = bankRepository;
    }

    public async Task<GetBanksResponse> Handle(GetBankByIdQuery query, CancellationToken ct)
    {
        var id = BankId.Of(query.BankId);
        var entity = await _bankRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Bank with id '{query.BankId}' not found.");

        return entity.Adapt<GetBanksResponse>();
    }
} 