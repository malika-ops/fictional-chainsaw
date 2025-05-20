using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

public class InvalidTypeDefinitionDeletingException : BusinessException
{
    public InvalidTypeDefinitionDeletingException(string validationMessage)
        : base(validationMessage)
    {
    }
}