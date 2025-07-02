using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ContractDetails.Commands.CreateContractDetails;

public record CreateContractDetailsCommand : ICommand<Result<Guid>>
{
    public Guid ContractId { get; init; }
    public Guid PricingId { get; init; }
}