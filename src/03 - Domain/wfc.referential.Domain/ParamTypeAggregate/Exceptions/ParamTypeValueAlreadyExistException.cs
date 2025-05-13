using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ParamTypeAggregate.Exceptions;

public class ParamTypeValueAlreadyExistException : BusinessException
{
    public ParamTypeValueAlreadyExistException(string value)
        : base($"ParamType with value {value} already exists for this TypeDefinition.")
    {
    }
}