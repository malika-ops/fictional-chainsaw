using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AffiliateAggregate;

namespace wfc.referential.Application.Affiliates.Commands.CreateAffiliate;

public record CreateAffiliateCommand : ICommand<Result<Guid>>
{
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
    public AffiliateTypeEnum AffiliateType { get; init; } 
}