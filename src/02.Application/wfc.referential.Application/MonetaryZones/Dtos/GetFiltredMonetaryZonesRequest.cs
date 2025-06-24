namespace wfc.referential.Application.MonetaryZones.Dtos;

public record GetFiltredMonetaryZonesRequest : FilterRequest
{

    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by description.</summary>
    public string? Description { get; init; }

}
