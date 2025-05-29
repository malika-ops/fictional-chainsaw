using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdatePartnerAccount;

public record UpdatePartnerAccountCommand : ICommand<Result<bool>>
{
    public Guid PartnerAccountId { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string RIB { get; init; } = string.Empty;
    public string? Domiciliation { get; init; }
    public string? BusinessName { get; init; }
    public string? ShortName { get; init; }
    public decimal AccountBalance { get; init; }
    public Guid BankId { get; init; }
    public Guid AccountTypeId { get; init; }
    public bool IsEnabled { get; init; } = true;
}