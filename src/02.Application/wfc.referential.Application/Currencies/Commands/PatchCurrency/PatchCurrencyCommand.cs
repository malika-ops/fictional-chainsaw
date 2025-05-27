using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Currencies.Commands.PatchCurrency;

public record PatchCurrencyCommand : ICommand<Result<bool>>
{
    public Guid CurrencyId { get; init; }
    public string? Code { get; init; }
    public string? CodeAR { get; init; }
    public string? CodeEN { get; init; }
    public string? Name { get; init; }
    public int? CodeIso { get; init; }
    public bool? IsEnabled { get; init; }
}