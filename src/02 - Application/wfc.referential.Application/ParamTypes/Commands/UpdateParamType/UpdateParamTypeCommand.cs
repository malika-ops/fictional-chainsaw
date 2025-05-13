using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.UpdateParamType;

public class UpdateParamTypeCommand : ICommand<Guid>
{
    public ParamTypeId ParamTypeId { get; set; }
    public string Value { get; set; }
    public bool IsEnabled { get; set; }
    public TypeDefinitionId TypeDefinitionId { get; set; }

    public UpdateParamTypeCommand(ParamTypeId paramTypeId, string value, bool isEnabled, TypeDefinitionId typeDefinitionId)
    {
        ParamTypeId = paramTypeId;
        Value = value;
        IsEnabled = isEnabled;
        TypeDefinitionId = typeDefinitionId;
    }
}