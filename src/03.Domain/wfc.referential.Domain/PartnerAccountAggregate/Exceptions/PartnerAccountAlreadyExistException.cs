using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

public class PartnerAccountAlreadyExistException : BusinessException
{
    public PartnerAccountAlreadyExistException(string accountNumber) : base($"Partner account with account number {accountNumber} already exists.")
    {
    }
}
