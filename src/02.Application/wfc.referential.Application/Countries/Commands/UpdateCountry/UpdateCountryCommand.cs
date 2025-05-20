using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Countries.Commands.UpdateCountry;

public record UpdateCountryCommand : ICommand<Result<Guid>>
{
    public Guid CountryId { get; set; }

    public string Abbreviation { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string ISO2 { get; set; }
    public string ISO3 { get; set; }
    public string DialingCode { get; set; }
    public string TimeZone { get; set; }
    public bool HasSector { get; set; }
    public bool? IsSmsEnabled { get; set; }
    public int NumberDecimalDigits { get; set; }

    public Guid MonetaryZoneId { get; set; }
    public Guid CurrencyId { get; set; }
    public bool? IsEnabled { get; set; }


}
