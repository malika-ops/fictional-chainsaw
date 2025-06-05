using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.AffiliateAggregate.Exceptions;

public class AffiliateCodeAlreadyExistException(string code)
    : ConflictException($"Affiliate with code {code} already exists.");