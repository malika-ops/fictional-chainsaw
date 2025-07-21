using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.OperatorAggregate.Exceptions;

public class OperatorNotFoundException(Guid operatorId)
    : ResourceNotFoundException($"Operator with ID {operatorId} was not found.");