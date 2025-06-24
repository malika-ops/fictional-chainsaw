using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Countries.Commands.CreateCountry;

public record CreateCountryCommand : ICommand<Result<Guid>>
{
    public string? Abbreviation { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string ISO2 { get; init; } = string.Empty;
    public string ISO3 { get; init; } = string.Empty;
    public string DialingCode { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public bool HasSector { get; init; }
    public bool IsSmsEnabled { get; init; }
    public int NumberDecimalDigits { get; init; } 
    public bool IsEnabled { get; init; }

    public Guid MonetaryZoneId { get; init; }
    public Guid CurrencyId { get; init; } 
}
