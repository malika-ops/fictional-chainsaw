using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Countries.Commands.UpdateCountry;

public record UpdateCountryCommand : ICommand<Result<bool>>
{
    public Guid CountryId { get; set; }

    public string Abbreviation { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ISO2 { get; set; } = string.Empty;
    public string ISO3 { get; set; } = string.Empty;
    public string DialingCode { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool HasSector { get; set; }
    public bool IsSmsEnabled { get; set; }
    public int NumberDecimalDigits { get; set; }

    public Guid MonetaryZoneId { get; set; }
    public Guid CurrencyId { get; set; }
    public bool? IsEnabled { get; set; }


}
