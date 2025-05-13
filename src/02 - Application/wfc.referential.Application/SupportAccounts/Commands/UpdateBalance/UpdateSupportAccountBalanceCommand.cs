using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateBalance;

public record UpdateSupportAccountBalanceCommand : ICommand<Guid>
{
    public Guid SupportAccountId { get; set; }
    public decimal NewBalance { get; set; }

    public UpdateSupportAccountBalanceCommand(Guid supportAccountId, decimal newBalance)
    {
        SupportAccountId = supportAccountId;
        NewBalance = newBalance;
    }
}