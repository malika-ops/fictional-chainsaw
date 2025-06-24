using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ParamTypes.Commands.CreateParamType;

public record CreateParamTypeCommand : ICommand<Result<Guid>>
{
    public string Value { get; init; } = string.Empty;
    public Guid TypeDefinitionId { get; init; }
}