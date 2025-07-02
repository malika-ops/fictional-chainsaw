using Mapster;
using wfc.referential.Domain.ControleAggregate;

namespace wfc.referential.Application.Controles.Mappings;

public class ControleMappings
{
    public static void Register()
    {
        TypeAdapterConfig<ControleId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

    }
}
