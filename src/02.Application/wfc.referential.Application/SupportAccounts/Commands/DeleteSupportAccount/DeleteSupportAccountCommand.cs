using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.SupportAccounts.Commands.DeleteSupportAccount;

public record DeleteSupportAccountCommand : ICommand<Result<bool>>
{
    public Guid SupportAccountId { get; }
    public DeleteSupportAccountCommand(Guid supportAccountId) => SupportAccountId = supportAccountId;
}