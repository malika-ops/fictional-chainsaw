using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.SupportAccounts.Commands.DeleteSupportAccount;

public record DeleteSupportAccountCommand : ICommand<bool>
{
    public Guid SupportAccountId { get; set; }

    public DeleteSupportAccountCommand(Guid supportAccountId)
    {
        SupportAccountId = supportAccountId;
    }
}