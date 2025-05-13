using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

public class CodeAlreadyExistException : BusinessException
{
    public CodeAlreadyExistException(string code) : base($"MonetaryZone with code {code} already exists.")
    {
    }
}
