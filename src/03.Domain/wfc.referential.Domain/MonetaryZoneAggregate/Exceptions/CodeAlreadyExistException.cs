using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

public class CodeAlreadyExistException : ConflictException
{
    public CodeAlreadyExistException(string code) : base($"MonetaryZone with code {code} already exists.")
    {
    }
}
