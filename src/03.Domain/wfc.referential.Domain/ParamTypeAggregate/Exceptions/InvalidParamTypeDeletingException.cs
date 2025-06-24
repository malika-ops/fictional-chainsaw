using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ParamTypeAggregate.Exceptions;

public class InvalidParamTypeDeletingException(string validationMessage)
    : BusinessException(validationMessage);