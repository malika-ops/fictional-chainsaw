using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ParamTypeAggregate.Exceptions;

public class ParamTypeInUseException(Guid paramTypeId)
    : ConflictException($"Cannot delete ParamType with ID {paramTypeId} because it is currently in use.");