using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.Countries.Exceptions;

public class CountryCodeAlreadyExistException : BusinessException
{
    public CountryCodeAlreadyExistException(string code)
        : base($"Country with code {code} already exists.")
    {
    }
}