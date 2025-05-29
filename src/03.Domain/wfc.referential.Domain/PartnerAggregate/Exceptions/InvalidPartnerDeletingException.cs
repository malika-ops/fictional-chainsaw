using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PartnerAggregate.Exceptions;

public class InvalidPartnerDeletingException(string validationMessage)
    : BusinessException(validationMessage);