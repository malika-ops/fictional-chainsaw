namespace wfc.referential.Application.Currencies.Dtos;

public record GetAllCurrenciesRequest
{
    /// <summary>Optional page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Optional page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by Arabic code.</summary>
    public string? CodeAR { get; init; }

    /// <summary>Optional filter by English code.</summary>
    public string? CodeEN { get; init; }

    /// <summary>Optional filter by name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by 3-digit number.</summary>
    public int? CodeIso { get; init; }

    /// <summary>Optional filter by IsEnabled status.</summary>
    public bool? IsEnabled { get; init; } = true;
}