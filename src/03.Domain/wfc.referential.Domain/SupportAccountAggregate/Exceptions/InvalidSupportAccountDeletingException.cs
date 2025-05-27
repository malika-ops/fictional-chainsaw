using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SupportAccountAggregate.Exceptions;

public class InvalidSupportAccountDeletingException(string validationMessage)
    : BusinessException(validationMessage);