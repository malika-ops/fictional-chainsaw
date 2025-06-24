namespace wfc.referential.Application.Affiliates.Dtos;

public record DeleteAffiliateRequest
{
    /// <summary>
    /// The ID of the Affiliate to delete.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f8</example>
    public Guid AffiliateId { get; init; }
}