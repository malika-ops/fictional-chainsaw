namespace wfc.referential.Application.Affiliates.Dtos;

public record GetAffiliatesResponse
{
    public Guid AffiliateId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Abbreviation { get; init; } = string.Empty;
    public DateTime? OpeningDate { get; init; }
    public string CancellationDay { get; init; } = string.Empty;
    public string Logo { get; init; } = string.Empty;
    public decimal ThresholdBilling { get; init; }
    public string AccountingDocumentNumber { get; init; } = string.Empty;
    public string AccountingAccountNumber { get; init; } = string.Empty;
    public string StampDutyMention { get; init; } = string.Empty;
    public Guid CountryId { get; init; }
    public bool IsEnabled { get; init; }
    public Guid? AffiliateTypeId { get; init; }
}