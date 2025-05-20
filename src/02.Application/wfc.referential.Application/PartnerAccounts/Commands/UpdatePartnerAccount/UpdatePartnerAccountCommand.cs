using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdatePartnerAccount;

public class UpdatePartnerAccountCommand : ICommand<Guid>
{
    public Guid PartnerAccountId { get; }
    public string AccountNumber { get; }
    public string RIB { get; }
    public string? Domiciliation { get; }
    public string? BusinessName { get; }
    public string? ShortName { get; }
    public decimal AccountBalance { get; }
    public Guid BankId { get; }
    public Guid AccountTypeId { get; }
    public bool IsEnabled { get; }

    public UpdatePartnerAccountCommand(
        Guid partnerAccountId,
        string accountNumber,
        string rib,
        string? domiciliation,
        string? businessName,
        string? shortName,
        decimal accountBalance,
        Guid bankId,
        Guid accountTypeId,
        bool isEnabled)
    {
        PartnerAccountId = partnerAccountId;
        AccountNumber = accountNumber;
        RIB = rib;
        Domiciliation = domiciliation;
        BusinessName = businessName;
        ShortName = shortName;
        AccountBalance = accountBalance;
        BankId = bankId;
        AccountTypeId = accountTypeId;
        IsEnabled = isEnabled;
    }
}