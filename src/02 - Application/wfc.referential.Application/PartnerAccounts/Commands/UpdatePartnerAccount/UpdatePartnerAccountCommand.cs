using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Commands.UpdatePartnerAccount;

public class UpdatePartnerAccountCommand : ICommand<Guid>
{
    public Guid PartnerAccountId { get; set; }
    public string AccountNumber { get; set; }
    public string RIB { get; set; }
    public string Domiciliation { get; set; }
    public string BusinessName { get; set; }
    public string ShortName { get; set; }
    public decimal AccountBalance { get; set; }
    public Guid BankId { get; set; }
    public AccountType AccountType { get; set; }
    public bool IsEnabled { get; set; }

    public UpdatePartnerAccountCommand(
        Guid partnerAccountId,
        string accountNumber,
        string rib,
        string domiciliation,
        string businessName,
        string shortName,
        decimal accountBalance,
        Guid bankId,
        AccountType accountType,
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
        AccountType = accountType;
        IsEnabled = isEnabled;
    }
}