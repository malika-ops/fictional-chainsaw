using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.BankAggregate.Exceptions;

public class BankLinkedToAccountsException : BusinessException
{
    public BankLinkedToAccountsException(Guid bankId) : base($"Cannot delete bank with ID {bankId} because it is linked to one or more accounts.")
    {
    }
}