using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PartnerAggregate.Exceptions;

public class PartnerCodeAlreadyExistException(string code)
    : ConflictException($"Partner with code {code} already exists.");