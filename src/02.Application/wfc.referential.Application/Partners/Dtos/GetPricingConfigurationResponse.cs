using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.Partners.Dtos;

public record GetPricingConfigurationResponse
{
    public Guid PartnerId { get; init; }

    public ContractDto Contract { get; init; } = new();

    public PricingDto Pricing { get; init; } = new();

    public IReadOnlyList<TaxDto> Taxes { get; init; } = [];

    public record ContractDto
    {
        public Guid ContractId { get; init; }
        public string Code { get; init; } = string.Empty;
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset EndDate { get; init; }
    }

    public record PricingDto
    {
        public Guid PricingId { get; init; }
        public decimal? FixedAmount { get; init; }
        public decimal? Rate { get; init; }
        public decimal MinimumAmount { get; init; }
        public decimal MaximumAmount { get; init; }
        public string Channel { get; init; } = string.Empty;
        public Guid AffiliateId { get; init; }
        public Guid CorridorId { get; init; }
        public Guid ServiceId { get; init; }
    }

    public record TaxDto
    {
        public Guid TaxId { get; init; }
        public string TaxCode { get; init; } = string.Empty;
        public ApplicationRule AppliedOn { get; init; }
        public decimal Rate { get; init; }
        public decimal FixedAmount { get; init; }
    }
}
