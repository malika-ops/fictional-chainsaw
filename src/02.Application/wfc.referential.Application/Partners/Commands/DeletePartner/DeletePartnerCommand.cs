using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Partners.Commands.DeletePartner;

public record DeletePartnerCommand : ICommand<Result<bool>>
{
    public Guid PartnerId { get; init; }
}