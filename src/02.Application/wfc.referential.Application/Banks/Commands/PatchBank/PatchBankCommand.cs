using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Banks.Commands.PatchBank;

public record PatchBankCommand : ICommand<Result<bool>>
{
    public Guid BankId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Abbreviation { get; init; }
    public bool? IsEnabled { get; init; }
}