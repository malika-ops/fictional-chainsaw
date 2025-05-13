using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Commands.CreatePartnerAccount;

public class CreatePartnerAccountCommand : ICommand<Result<Guid>>
{
    public string AccountNumber { get; set; }
    public string RIB { get; set; }
    public string Domiciliation { get; set; }
    public string BusinessName { get; set; }
    public string ShortName { get; set; }
    public decimal AccountBalance { get; set; }
    public Guid BankId { get; set; }
    public AccountType AccountType { get; set; }

    public CreatePartnerAccountCommand(
        string accountNumber,
        string rib,
        string domiciliation,
        string businessName,
        string shortName,
        decimal accountBalance,
        Guid bankId,
        AccountType accountType)
    {
        AccountNumber = accountNumber;
        RIB = rib;
        Domiciliation = domiciliation;
        BusinessName = businessName;
        ShortName = shortName;
        AccountBalance = accountBalance;
        BankId = bankId;
        AccountType = accountType;
    }
}