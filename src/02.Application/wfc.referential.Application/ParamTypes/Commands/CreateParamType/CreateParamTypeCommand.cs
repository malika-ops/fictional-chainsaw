using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.CreateParamType;

public record CreateParamTypeCommand : ICommand<Result<Guid>>
{
    public ParamTypeId ParamTypeId { get; init; }
    public string Value { get; init; }
    public TypeDefinitionId TypeDefinitionId { get; init; }

    public CreateParamTypeCommand(
        ParamTypeId paramTypeId,
        string value,
        TypeDefinitionId typeDefinitionId)
    {
        ParamTypeId = paramTypeId;
        Value = value;
        TypeDefinitionId = typeDefinitionId;
    }
}