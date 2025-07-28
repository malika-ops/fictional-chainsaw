using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Currencies.Commands.UpdateCurrency;

public record UpdateCurrencyCommand : ICommand<Result<bool>>
{
    public Guid CurrencyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CodeAR { get; set; } = string.Empty;
    public string CodeEN { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CodeIso { get; set; }
    public bool IsEnabled { get; set; } = true;
}