using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

public class InvalidTypeDefinitionDeletingException(string validationMessage)
    : BusinessException(validationMessage);