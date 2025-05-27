using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.TierAggregate.Exceptions;

public class TierNameAlreadyExistException(string exceptionMessage)
    : ConflictException(exceptionMessage);