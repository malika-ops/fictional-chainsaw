using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.BankAggregate.Exceptions;

public class InvalidBankDeletingException(string validationMessage)
    : BusinessException(validationMessage);