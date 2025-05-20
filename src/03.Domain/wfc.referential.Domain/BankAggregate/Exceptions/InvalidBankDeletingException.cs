using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.BankAggregate.Exceptions;

public class InvalidBankDeletingException : BusinessException
{
    public InvalidBankDeletingException(string validationMessage) : base(validationMessage)
    {
    }
}
