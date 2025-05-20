using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

public class InvalidDeletingException : BusinessException
{
    public InvalidDeletingException(string validationMessage) : base(validationMessage)
    {
    }
}