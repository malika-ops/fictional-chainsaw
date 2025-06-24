using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ParamTypes.Commands.UpdateParamType;

public record UpdateParamTypeCommand : ICommand<Result<bool>>
{
    public Guid ParamTypeId { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public Guid TypeDefinitionId { get; set; }
}