using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AffiliateAggregate;

namespace wfc.referential.Application.Affiliates.Commands.PatchAffiliate;

public record PatchAffiliateCommand : ICommand<Result<bool>>
{
    public Guid AffiliateId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Abbreviation { get; init; }
    public DateTime? OpeningDate { get; init; }
    public string? CancellationDay { get; init; }
    public string? Logo { get; init; }
    public decimal? ThresholdBilling { get; init; }
    public string? AccountingDocumentNumber { get; init; }
    public string? AccountingAccountNumber { get; init; }
    public string? StampDutyMention { get; init; }
    public Guid? CountryId { get; init; }
    public bool? IsEnabled { get; init; }
    public AffiliateTypeEnum? AffiliateType { get; init; }
}