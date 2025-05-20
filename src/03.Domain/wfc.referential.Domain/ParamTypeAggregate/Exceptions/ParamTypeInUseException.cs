using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ParamTypeAggregate.Exceptions;

public class ParamTypeInUseException : BusinessException
{
    public ParamTypeInUseException(Guid paramTypeId)
        : base($"Cannot delete ParamType with ID {paramTypeId} because it is currently in use.")
    {
    }
}