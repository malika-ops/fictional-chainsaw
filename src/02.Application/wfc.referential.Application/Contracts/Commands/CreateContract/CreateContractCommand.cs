using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Contracts.Commands.CreateContract;

public record CreateContractCommand : ICommand<Result<Guid>>
{
    public string Code { get; init; } = string.Empty;
    public Guid PartnerId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
