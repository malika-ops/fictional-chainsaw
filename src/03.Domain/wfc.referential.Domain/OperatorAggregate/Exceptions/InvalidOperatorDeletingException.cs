using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.OperatorAggregate.Exceptions;

public class InvalidOperatorDeletingException(string validationMessage)
    : ResourceNotFoundException(validationMessage);