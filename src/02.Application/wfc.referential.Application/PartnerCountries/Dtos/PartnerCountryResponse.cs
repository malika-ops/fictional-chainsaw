namespace wfc.referential.Application.PartnerCountries.Dtos;

public record PartnerCountryResponse
{
    /// <summary>
    /// Unique identifier of the partner-country association.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid PartnerCountryId { get; init; }

    /// <summary>
    /// Unique identifier of the partner.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid PartnerId { get; init; }

    /// <summary>
    /// Unique identifier of the country.
    /// </summary>
    /// <example>0a1b2c3d-4e5f-6789-abcd-ef0123456789</example>
    public Guid CountryId { get; init; }

    /// <summary>
    /// Indicates whether the association is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Date and time when the association was created.
    /// </summary>
    /// <example>2024-01-01T12:00:00+00:00</example>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Date and time when the association was last modified.
    /// </summary>
    /// <example>2024-06-01T12:00:00+00:00</example>
    public DateTimeOffset LastModified { get; init; }

    /// <summary>
    /// User who created the association.
    /// </summary>
    /// <example>admin</example>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// User who last modified the association.
    /// </summary>
    /// <example>editor</example>
    public string LastModifiedBy { get; init; }
}