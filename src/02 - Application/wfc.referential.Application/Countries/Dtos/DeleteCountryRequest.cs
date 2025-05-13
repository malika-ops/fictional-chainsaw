using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Countries.Dtos;

public record DeleteCountryRequest
{
    /// <summary>
    /// The GUID of the Country to delete.
    /// </summary>
    [Required]
    public Guid CountryId { get; init; }
}
