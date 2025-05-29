using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.TaxAggregate.Exceptions;

public class CodeAlreadyExistException : ConflictException
{
    public CodeAlreadyExistException(string code): base($"{nameof(Tax)} with code : {code} already exist")
    {
        
    }
}
