using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.TaxAggregate.Exceptions;

public class CodeAlreadyExistException : BusinessException
{
    public CodeAlreadyExistException(string code): base($"{nameof(Tax)} with code : {code} already exist")
    {
        
    }
}
