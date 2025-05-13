using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.Banks.Commands.PatchBank;

public class PatchBankCommand : ICommand<Guid>
{
    // The ID from the route
    public Guid BankId { get; }

    // The optional fields to update
    public string? Code { get; }
    public string? Name { get; }
    public string? Abbreviation { get; }
    public bool? IsEnabled { get; }

    public PatchBankCommand(
        Guid bankId,
        string? code = null,
        string? name = null,
        string? abbreviation = null,
        bool? isEnabled = null)
    {
        BankId = bankId;
        Code = code;
        Name = name;
        Abbreviation = abbreviation;
        IsEnabled = isEnabled;
    }
}