using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.PartnerAccounts.Commands.DeletePartnerAccount;

public record DeletePartnerAccountCommand : ICommand<Result<bool>>
{
    public Guid PartnerAccountId { get; }
    public DeletePartnerAccountCommand(Guid partnerAccountId) => PartnerAccountId = partnerAccountId;
}