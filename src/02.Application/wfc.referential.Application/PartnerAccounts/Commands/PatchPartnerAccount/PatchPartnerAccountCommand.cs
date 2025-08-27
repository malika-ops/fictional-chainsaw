using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Commands.PatchPartnerAccount;

public record PatchPartnerAccountCommand : ICommand<Result<bool>>
{
    public Guid PartnerAccountId { get; init; }
    public string? AccountNumber { get; init; }
    public string? RIB { get; init; }
    public string? Domiciliation { get; init; }
    public string? BusinessName { get; init; }
    public string? ShortName { get; init; }
    public decimal? AccountBalance { get; init; }
    public Guid? BankId { get; init; }
    public PartnerAccountTypeEnum? PartnerAccountType { get; init; }
    public bool? IsEnabled { get; init; }
}