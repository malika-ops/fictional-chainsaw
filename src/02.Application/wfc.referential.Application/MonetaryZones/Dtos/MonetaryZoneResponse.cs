using wfc.referential.Application.Countries.Dtos;

namespace wfc.referential.Application.MonetaryZones.Dtos;

public record MonetaryZoneResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public List<GetCountriesResponce>? Countries { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string LastModifiedBy { get; init; } = string.Empty;

}
