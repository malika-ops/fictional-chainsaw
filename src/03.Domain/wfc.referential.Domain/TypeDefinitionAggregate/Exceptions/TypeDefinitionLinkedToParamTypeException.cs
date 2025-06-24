using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

public class TypeDefinitionLinkedToParamTypeException(Guid typeDefinitionId)
    : ConflictException($"Cannot delete TypeDefinition with ID {typeDefinitionId} because it is linked to one or more ParamTypes.");