using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CorridorAggregate.Exceptions;

public class CodeAlreadyExistException : BusinessException
{
    public CodeAlreadyExistException(string code): base($"{nameof(Corridor)} with code : {code} already exist")
    {
        
    }
}
