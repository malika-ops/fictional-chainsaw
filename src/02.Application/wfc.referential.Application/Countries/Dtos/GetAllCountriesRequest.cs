namespace wfc.referential.Application.Countries.Dtos;

public record GetAllCountriesRequest
{
    /// <summary>Optional page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Optional page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Optional filter by country name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by country code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by ISO2 code.</summary>
    public string? ISO2 { get; init; }

    /// <summary>Optional filter by ISO3 code.</summary>
    public string? ISO3 { get; init; }

    /// <summary>Optional filter by country status.</summary>
    public bool? IsEnabled { get; init; } = true;

    /// <summary>Optional filter by MonetaryZoneId.</summary>
    public Guid? MonetaryZoneId { get; init; }
}
