using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Commands.PatchPartnerAccount;

public class PatchPartnerAccountCommand : ICommand<Guid>
{
    // The ID from the route
    public Guid PartnerAccountId { get; }

    // The optional fields to update
    public string? AccountNumber { get; }
    public string? RIB { get; }
    public string? Domiciliation { get; }
    public string? BusinessName { get; }
    public string? ShortName { get; }
    public decimal? AccountBalance { get; }
    public Guid? BankId { get; }
    public AccountType? AccountType { get; }
    public bool? IsEnabled { get; }

    public PatchPartnerAccountCommand(
        Guid partnerAccountId,
        string? accountNumber = null,
        string? rib = null,
        string? domiciliation = null,
        string? businessName = null,
        string? shortName = null,
        decimal? accountBalance = null,
        Guid? bankId = null,
        AccountType? accountType = null,
        bool? isEnabled = null)
    {
        PartnerAccountId = partnerAccountId;
        AccountNumber = accountNumber;
        RIB = rib;
        Domiciliation = domiciliation;
        BusinessName = businessName;
        ShortName = shortName;
        AccountBalance = accountBalance;
        BankId = bankId;
        AccountType = accountType;
        IsEnabled = isEnabled;
    }
}