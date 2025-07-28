using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Currencies.Commands.CreateCurrency;

public record CreateCurrencyCommand : ICommand<Result<Guid>>
{
    public string Code { get; init; } = string.Empty;
    public string CodeAR { get; init; } = string.Empty;
    public string CodeEN { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int CodeIso { get; init; }
}