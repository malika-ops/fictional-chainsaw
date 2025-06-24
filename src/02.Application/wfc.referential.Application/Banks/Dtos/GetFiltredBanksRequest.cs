namespace wfc.referential.Application.Banks.Dtos;
public record GetFiltredBanksRequest : FilterRequest
{
    /// <summary>Filter by bank code.</summary>
    public string? Code { get; init; }

    /// <summary>Filter by bank name.</summary>
    public string? Name { get; init; }

    /// <summary>Filter by abbreviation.</summary>
    public string? Abbreviation { get; init; }

}
