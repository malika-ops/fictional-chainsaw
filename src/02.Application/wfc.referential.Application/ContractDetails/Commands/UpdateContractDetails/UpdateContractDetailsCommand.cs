using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ContractDetails.Commands.UpdateContractDetails;

public record UpdateContractDetailsCommand : ICommand<Result<bool>>
{
    public Guid ContractDetailsId { get; init; }
    public Guid ContractId { get; init; }
    public Guid PricingId { get; init; }
    public bool IsEnabled { get; init; } = true;
}