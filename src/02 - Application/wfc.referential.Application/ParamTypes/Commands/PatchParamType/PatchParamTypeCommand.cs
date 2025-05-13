using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.PatchParamType;

public class PatchParamTypeCommand : ICommand<Result<Guid>>
{
    // The ID from the route
    public Guid ParamTypeId { get; }
    public TypeDefinitionId TypeDefinitionId { get; }

    // The optional fields to update
    public string? Value { get; }
    public bool? IsEnabled { get; }

    public PatchParamTypeCommand(
        Guid paramTypeId,
        TypeDefinitionId typeDefinitionId,
        string? value,
        bool? isEnabled)
    {
        ParamTypeId = paramTypeId;
        TypeDefinitionId = typeDefinitionId;
        Value = value;
        IsEnabled = isEnabled;
    }
}