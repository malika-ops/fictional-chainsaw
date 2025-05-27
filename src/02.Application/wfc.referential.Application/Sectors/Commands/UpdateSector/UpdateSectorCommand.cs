using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Sectors.Commands.UpdateSector;

public record UpdateSectorCommand : ICommand<Result<bool>>
{
    public Guid SectorId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CityId { get; set; }
    public bool IsEnabled { get; set; } = true;
}