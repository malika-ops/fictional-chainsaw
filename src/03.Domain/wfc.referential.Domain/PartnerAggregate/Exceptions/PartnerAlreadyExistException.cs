using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PartnerAggregate.Exceptions;

public class PartnerAlreadyExistException : BusinessException
{
    public PartnerAlreadyExistException(string code) : base($"Partner with code {code} already exists.")
    {
    }
}