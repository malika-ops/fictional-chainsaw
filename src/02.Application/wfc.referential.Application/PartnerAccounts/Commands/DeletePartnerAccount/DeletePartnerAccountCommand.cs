using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.PartnerAccounts.Commands.DeletePartnerAccount;

public class DeletePartnerAccountCommand : ICommand<bool>
{
    public Guid PartnerAccountId { get; set; }

    public DeletePartnerAccountCommand(Guid partnerAccountId)
    {
        PartnerAccountId = partnerAccountId;
    }
}