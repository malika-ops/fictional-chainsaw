namespace wfc.referential.Application.Controles.Dtos;

public record GetFilteredControlesRequest : FilterRequest
{
    /// <summary>Filter by Code.</summary>
    public string? Code { get; init; }

    /// <summary>Filter by Name.</summary>
    public string? Name { get; init; }
}
