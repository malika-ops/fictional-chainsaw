using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.IdentityDocumentAggregate.Exceptions;

public class CodeAlreadyExistException : BusinessException
{
    public CodeAlreadyExistException(string code)
        : base($"{nameof(IdentityDocument)} with code : {code} already exists")
    {
    }
}