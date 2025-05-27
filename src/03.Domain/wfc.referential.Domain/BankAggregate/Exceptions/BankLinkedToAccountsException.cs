using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.BankAggregate.Exceptions;

public class BankLinkedToAccountsException(Guid bankId)
    : ConflictException($"Cannot delete bank with ID {bankId} because it is linked to one or more accounts.");