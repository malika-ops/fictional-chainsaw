using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.RegionAggregate.Exceptions;

public class CodeAlreadyExistException : ConflictException
{
    public CodeAlreadyExistException(string code): base($"{nameof(Region)} with code : {code} already exist")
    {
        
    }
}
