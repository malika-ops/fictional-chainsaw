using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.RegionAggregate.Exceptions;

public class CodeAlreadyExistException : BusinessException
{
    public CodeAlreadyExistException(string code): base($"{nameof(Region)} with code : {code} already exist")
    {
        
    }
}
