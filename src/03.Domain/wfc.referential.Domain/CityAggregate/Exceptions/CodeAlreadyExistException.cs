using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CityAggregate.Exceptions;

public class CodeAlreadyExistException : ConflictException
{
    public CodeAlreadyExistException(string code): base($"{nameof(City)} with code : {code} already exist")
    {
        
    }
}
