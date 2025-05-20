using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

public class InvalidPartnerAccountDeletingException : BusinessException
{
    public InvalidPartnerAccountDeletingException(string validationMessage) : base(validationMessage)
    {
    }
}