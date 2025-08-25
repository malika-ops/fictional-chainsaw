using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AffiliateAggregate;

namespace wfc.referential.Application.Affiliates.Commands.UpdateAffiliate;

public record UpdateAffiliateCommand : ICommand<Result<bool>>
{
    public Guid AffiliateId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Abbreviation { get; init; } = string.Empty;
    public DateTime OpeningDate { get; init; } 
    public string CancellationDay { get; init; } = string.Empty;
    public string Logo { get; init; } = string.Empty;
    public decimal ThresholdBilling { get; init; }
    public string AccountingDocumentNumber { get; init; } = string.Empty;
    public string AccountingAccountNumber { get; init; } = string.Empty;
    public string StampDutyMention { get; init; } = string.Empty;
    public Guid CountryId { get; init; }
    public bool IsEnabled { get; init; } = true;
    public AffiliateTypeEnum AffiliateType { get; init; } 
}