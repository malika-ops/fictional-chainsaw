using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.Countries.Exceptions;

public class CountryCodeAlreadyExistException : ConflictException
{
    public CountryCodeAlreadyExistException(string code)
        : base($"Country with code {code} already exists.")
    {
    }
}