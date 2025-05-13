using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.Sectors.Commands.PatchSector;

public class PatchSectorCommand : ICommand<Guid>
{
    // The ID from the route
    public Guid SectorId { get; }

    // The optional fields to update
    public string? Code { get; }
    public string? Name { get; }
    public Guid? CityId { get; }
    public bool? IsEnabled { get; }

    public PatchSectorCommand(
        Guid sectorId,
        string? code = null,
        string? name = null,
        Guid? cityId = null,
        bool? isEnabled = null
        )
    {
        SectorId = sectorId;
        Code = code;
        Name = name;
        CityId = cityId;
        IsEnabled = isEnabled;
    }
}