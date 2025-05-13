using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SupportAccountAggregate.Exceptions;

public class InvalidSupportAccountDeletingException : BusinessException
{
    public InvalidSupportAccountDeletingException(string validationMessage) : base(validationMessage)
    {
    }
}