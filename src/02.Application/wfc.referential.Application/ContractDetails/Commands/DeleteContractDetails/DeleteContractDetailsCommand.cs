using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ContractDetails.Commands.DeleteContractDetails;

public record DeleteContractDetailsCommand : ICommand<Result<bool>>
{
    public Guid ContractDetailsId { get; init; }
}