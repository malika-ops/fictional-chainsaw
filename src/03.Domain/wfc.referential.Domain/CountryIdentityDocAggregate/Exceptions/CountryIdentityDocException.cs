using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CountryIdentityDocAggregate.Exceptions;

public class CountryIdentityDocException : BusinessException
{
    public CountryIdentityDocException(string message) : base(message)
    {
    }
}