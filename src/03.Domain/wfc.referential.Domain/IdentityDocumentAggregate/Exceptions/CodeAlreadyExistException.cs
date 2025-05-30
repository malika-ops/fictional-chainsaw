using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.IdentityDocumentAggregate.Exceptions;

public class IdentityDocumentCodeAlreadyExistException(string code)
    : ConflictException($"Identity document with code {code} already exists.");