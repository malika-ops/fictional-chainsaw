using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Agencies.Dtos;

public record DeleteAgencyRequest
{
    /// <summary>GUID of the agency to delete.</summary>
    [Required]
    public Guid AgencyId { get; init; }
}
