using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.PatchParamType;

public record PatchParamTypeCommand(Guid ParamTypeId, TypeDefinitionId TypeDefinitionId, string? Value, bool? IsEnabled) 
    : ICommand<Result<Guid>>;