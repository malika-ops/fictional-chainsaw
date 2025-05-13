using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.MonetaryZones.Dtos;

public record DeleteMonetaryZoneRequest
{
    /// <summary>
    /// The string representation of the MonetaryZone's GUID (route param).
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    [Required]
    public Guid MonetaryZoneId { get; init; }
}
