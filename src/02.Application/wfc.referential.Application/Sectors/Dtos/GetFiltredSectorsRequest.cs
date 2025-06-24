namespace wfc.referential.Application.Sectors.Dtos;

public record GetFiltredSectorsRequest : FilterRequest
{
    /// <summary>Filter by sector code.</summary>
    public string? Code { get; init; }

    /// <summary>Filter by sector name.</summary>
    public string? Name { get; init; }

    /// <summary>Filter by city ID.</summary>
    public Guid? CityId { get; init; }
}