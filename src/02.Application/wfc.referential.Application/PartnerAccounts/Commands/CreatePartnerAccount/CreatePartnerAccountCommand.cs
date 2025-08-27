using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Commands.CreatePartnerAccount;

public record CreatePartnerAccountCommand : ICommand<Result<Guid>>
{
    public string AccountNumber { get; init; } = string.Empty;
    public string RIB { get; init; } = string.Empty;
    public string? Domiciliation { get; init; }
    public string? BusinessName { get; init; }
    public string? ShortName { get; init; }
    public decimal AccountBalance { get; init; }
    public Guid BankId { get; init; }
    public PartnerAccountTypeEnum PartnerAccountType { get; init; }
}