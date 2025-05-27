using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateBalance;

public record UpdateSupportAccountBalanceCommand : ICommand<Result<bool>>
{
    public Guid SupportAccountId { get; init; }
    public decimal NewBalance { get; init; }
}