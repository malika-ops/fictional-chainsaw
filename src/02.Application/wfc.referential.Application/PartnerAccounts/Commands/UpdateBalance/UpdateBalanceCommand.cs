using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdateBalance;

public record UpdateBalanceCommand : ICommand<Result<bool>>
{
    public Guid PartnerAccountId { get; init; }
    public decimal NewBalance { get; init; }
}