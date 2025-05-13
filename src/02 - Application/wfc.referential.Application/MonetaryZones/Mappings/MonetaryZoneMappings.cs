using Mapster;
using wfc.referential.Application.MonetaryZones.Commands.CreateMonetaryZone;
using wfc.referential.Application.MonetaryZones.Commands.DeleteMonetaryZone;
using wfc.referential.Application.MonetaryZones.Commands.PatchMonetaryZone;
using wfc.referential.Application.MonetaryZones.Commands.UpdateMonetaryZone;
using wfc.referential.Application.MonetaryZones.Dtos;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.MonetaryZones.Mappings;

public class MonetaryZoneMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CreateMonetaryZoneRequest, CreateMonetaryZoneCommand>
            .NewConfig()
            .ConstructUsing(src => new CreateMonetaryZoneCommand(src.Code,src.Name,src.Description));

        TypeAdapterConfig<DeleteMonetaryZoneRequest, DeleteMonetaryZoneCommand>
            .NewConfig()
            .ConstructUsing(src => new DeleteMonetaryZoneCommand(src.MonetaryZoneId));

        TypeAdapterConfig<UpdateMonetaryZoneRequest, UpdateMonetaryZoneCommand>
            .NewConfig()
            .ConstructUsing(src => new UpdateMonetaryZoneCommand(src.MonetaryZoneId, src.Code, src.Name, src.Description, src.IsEnabled));

        TypeAdapterConfig<PatchMonetaryZoneRequest, PatchMonetaryZoneCommand>
            .NewConfig()
            .ConstructUsing(src => new PatchMonetaryZoneCommand(src.MonetaryZoneId ,src.Code, src.Name, src.Description,src.IsEnabled));

        TypeAdapterConfig<PatchMonetaryZoneCommand, MonetaryZone>
            .NewConfig()
            .IgnoreNullValues(true);

    }
}
