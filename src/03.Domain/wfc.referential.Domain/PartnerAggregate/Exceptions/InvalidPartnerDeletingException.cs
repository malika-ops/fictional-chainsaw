using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PartnerAggregate.Exceptions;

public class InvalidPartnerDeletingException : BusinessException
{
    public InvalidPartnerDeletingException(string validationMessage) : base(validationMessage)
    {
    }
}