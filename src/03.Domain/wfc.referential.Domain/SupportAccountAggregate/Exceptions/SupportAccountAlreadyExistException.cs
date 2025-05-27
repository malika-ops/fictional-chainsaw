using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SupportAccountAggregate.Exceptions;

public class SupportAccountCodeAlreadyExistException(string code)
    : ConflictException($"Support account with code {code} already exists.");