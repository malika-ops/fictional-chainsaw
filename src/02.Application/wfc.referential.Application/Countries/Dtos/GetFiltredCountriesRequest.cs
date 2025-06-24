namespace wfc.referential.Application.Countries.Dtos;

public record GetFiltredCountriesRequest : FilterRequest
{
    /// <summary>Optional filter by country name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by country code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by ISO2 code.</summary>
    public string? ISO2 { get; init; }

    /// <summary>Optional filter by ISO3 code.</summary>
    public string? ISO3 { get; init; }

    /// <summary>Optional filter by MonetaryZoneId.</summary>
    public Guid? MonetaryZoneId { get; init; }
}
