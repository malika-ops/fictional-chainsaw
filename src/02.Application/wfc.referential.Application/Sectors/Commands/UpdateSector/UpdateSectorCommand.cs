using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.Sectors.Commands.UpdateSector;

public class UpdateSectorCommand : ICommand<Guid>
{
    public Guid SectorId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public Guid CityId { get; set; }
    public bool IsEnabled { get; set; }

    public UpdateSectorCommand(Guid sectorId, string code, string name, Guid cityId, bool isEnabled = true)
    {
        SectorId = sectorId;
        Code = code;
        Name = name;
        CityId = cityId;
        IsEnabled = isEnabled;
    }
}