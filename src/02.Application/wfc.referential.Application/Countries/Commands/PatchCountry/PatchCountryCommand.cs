using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Countries.Commands.PatchCountry;

public record PatchCountryCommand : ICommand<Result<bool>>
{
    public Guid CountryId { get; init; }
    public string? Abbreviation { get; init; }
    public string? Name { get; init; }
    public string? Code { get; init; }
    public string? ISO2 { get; init; }
    public string? ISO3 { get; init; }
    public string? DialingCode { get; init; }
    public string? TimeZone { get; init; }
    public bool? HasSector { get; init; }
    public bool? IsSmsEnabled { get; init; }
    public int? NumberDecimalDigits { get; init; }
    public Guid? MonetaryZoneId { get; init; }
    public Guid? CurrencyId { get; init; }
    public bool? IsEnabled { get; init; }


}
