namespace wfc.referential.Application.Partners.Dtos;

public record DeletePartnerRequest
{
    /// <summary>
    /// The ID of the Partner to delete.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f8</example>
    public Guid PartnerId { get; init; }
}