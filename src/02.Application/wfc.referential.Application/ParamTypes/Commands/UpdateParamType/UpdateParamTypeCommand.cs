using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.UpdateParamType;

public record UpdateParamTypeCommand(ParamTypeId ParamTypeId, string Value, bool IsEnabled, TypeDefinitionId TypeDefinitionId) 
    : ICommand<Guid>;