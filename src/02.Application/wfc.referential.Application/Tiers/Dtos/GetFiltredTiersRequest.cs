namespace wfc.referential.Application.Tiers.Dtos;

public record GetFiltredTiersRequest : FilterRequest
{
    /// <summary>Optional filter by name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by description.</summary>
    public string? Description { get; init; }

}
