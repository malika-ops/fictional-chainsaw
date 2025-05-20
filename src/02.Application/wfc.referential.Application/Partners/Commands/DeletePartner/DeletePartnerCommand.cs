using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.Partners.Commands.DeletePartner;

public record DeletePartnerCommand : ICommand<bool>
{
    public Guid PartnerId { get; set; }

    public DeletePartnerCommand(Guid partnerId)
    {
        PartnerId = partnerId;
    }
}