using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdateBalance;

public class UpdateBalanceCommand : ICommand<Guid>
{
    public Guid PartnerAccountId { get; set; }
    public decimal NewBalance { get; set; }

    public UpdateBalanceCommand(Guid partnerAccountId, decimal newBalance)
    {
        PartnerAccountId = partnerAccountId;
        NewBalance = newBalance;
    }
}