using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Contracts.Commands.UpdateContract;

public record UpdateContractCommand : ICommand<Result<bool>>
{
    public Guid ContractId { get; init; }
    public string Code { get; init; } = string.Empty;
    public Guid PartnerId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsEnabled { get; init; } = true;
}