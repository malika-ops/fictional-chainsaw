using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ProductAggregate.Exceptions;

public class CodeAlreadyExistException : ConflictException
{
    public CodeAlreadyExistException(string code): base($"{nameof(Product)} with code : {code} already exist")
    {
        
    }
}
