namespace wfc.referential.Application.Countries.Dtos;

public record GetAllCountriesRequest
{
    /// <summary>Optional page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Optional page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Optional filter by country abbreviation.</summary>
    public string? Abbreviation { get; init; }

    /// <summary>Optional filter by country name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by country code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by ISO2 code.</summary>
    public string? ISO2 { get; init; }

    /// <summary>Optional filter by ISO3 code.</summary>
    public string? ISO3 { get; init; }

    /// <summary>Optional filter by dialing code.</summary>
    public string? DialingCode { get; init; }

    /// <summary>Optional filter by time zone.</summary>
    public string? TimeZone { get; init; }

    /// <summary>Optional filter by SMS enabled status.</summary>
    public bool? IsSmsEnabled { get; init; }

    /// <summary>Optional filter by IsSmsEnabled.</summary>
    public bool? HasSector { get; init; }

    /// <summary>Optional filter by number of decimal digits.</summary>
    public int? NumberDecimalDigits { get; init; }

    /// <summary>Optional filter by country status.</summary>
    public bool? IsEnabled { get; init; } = true;
}
