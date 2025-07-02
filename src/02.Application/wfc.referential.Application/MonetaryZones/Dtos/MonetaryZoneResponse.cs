using wfc.referential.Application.Countries.Dtos;

namespace wfc.referential.Application.MonetaryZones.Dtos;

public record MonetaryZoneResponse
{
    /// <summary>
    /// Unique identifier of the monetary zone.
    /// </summary>
    /// <example>e3b0c442-98fc-1c14-9afb-4c1a1e1f2a3b</example>
    public Guid Id { get; init; }

    /// <summary>
    /// Unique code of the monetary zone.
    /// </summary>
    /// <example>ZONE001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the monetary zone.
    /// </summary>
    /// <example>West African Monetary Zone</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of the monetary zone.
    /// </summary>
    /// <example>Monetary zone for West African countries</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the monetary zone is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// List of countries in the monetary zone.
    /// </summary>
    public List<GetCountriesResponce>? Countries { get; init; }

    /// <summary>
    /// Date and time when the monetary zone was created.
    /// </summary>
    /// <example>2024-01-01T12:00:00+00:00</example>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Date and time when the monetary zone was last modified.
    /// </summary>
    /// <example>2024-06-01T12:00:00+00:00</example>
    public DateTimeOffset LastModified { get; init; }

    /// <summary>
    /// User who created the monetary zone.
    /// </summary>
    /// <example>admin</example>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// User who last modified the monetary zone.
    /// </summary>
    /// <example>editor</example>
    public string LastModifiedBy { get; init; } = string.Empty;
}
