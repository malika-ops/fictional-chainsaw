using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Contracts.Commands.DeleteContract;

public record DeleteContractCommand : ICommand<Result<bool>>
{
    public Guid ContractId { get; init; }
}