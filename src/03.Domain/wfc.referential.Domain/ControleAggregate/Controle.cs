using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ControleAggregate;

public class Controle : Aggregate<ControleId>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;

    private Controle() { }

}