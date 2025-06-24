using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ContractAggregate.Exceptions;

public class InvalidContractDeletingException(string validationMessage)
    : ResourceNotFoundException(validationMessage);