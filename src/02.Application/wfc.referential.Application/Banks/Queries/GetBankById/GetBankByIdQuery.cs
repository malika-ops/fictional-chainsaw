using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Banks.Dtos;

namespace wfc.referential.Application.Banks.Queries.GetBankById;

public record GetBankByIdQuery : IQuery<GetBanksResponse>
{
    public Guid BankId { get; init; }
} 