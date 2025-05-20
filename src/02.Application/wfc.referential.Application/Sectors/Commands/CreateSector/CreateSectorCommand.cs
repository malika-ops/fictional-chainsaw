using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Sectors.Commands.CreateSector;

public class CreateSectorCommand : ICommand<Result<Guid>>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public Guid CityId { get; set; }

    public CreateSectorCommand(string code, string name, Guid cityId)
    {
        Code = code;
        Name = name;
        CityId = cityId;
    }
}