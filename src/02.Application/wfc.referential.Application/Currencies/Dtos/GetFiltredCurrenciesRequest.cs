namespace wfc.referential.Application.Currencies.Dtos;
public record GetFiltredCurrenciesRequest : FilterRequest
{

    /// <summary>Filter by currency code.</summary>
    public string? Code { get; init; }

    /// <summary>Filter by Arabic code.</summary>
    public string? CodeAR { get; init; }

    /// <summary>Filter by English code.</summary>
    public string? CodeEN { get; init; }

    /// <summary>Filter by currency name.</summary>
    public string? Name { get; init; }

    /// <summary>Filter by ISO code.</summary>
    public int? CodeIso { get; init; }
}