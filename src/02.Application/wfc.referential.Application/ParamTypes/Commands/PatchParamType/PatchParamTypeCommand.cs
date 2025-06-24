using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ParamTypes.Commands.PatchParamType;

public record PatchParamTypeCommand : ICommand<Result<bool>>
{
    public Guid ParamTypeId { get; init; }
    public string? Value { get; init; }
    public bool? IsEnabled { get; init; }
}