using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Pricings.Dtos;

public record DeletePricingRequest
{
    /// <summary>GUID of the pricing row to delete (route param).</summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    [Required] public Guid PricingId { get; init; }
}
