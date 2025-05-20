using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ServiceAggregate.Exceptions;

public class CodeAlreadyExistException : BusinessException
{
    public CodeAlreadyExistException(string code)
        : base($"{nameof(Service)} with code : {code} already exists")
    {
    }
}