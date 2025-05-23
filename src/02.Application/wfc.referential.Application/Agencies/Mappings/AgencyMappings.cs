using Mapster;
using wfc.referential.Application.Agencies.Dtos;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.SectorAggregate;

namespace wfc.referential.Application.Agencies.Mappings;

public class AgencyMappings
{
    public static void Register()
    {

        TypeAdapterConfig<AgencyId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<CityId, Guid>
            .NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<SectorId, Guid?>
            .NewConfig()
            .MapWith(src => src != null ? src.Value : null);

        TypeAdapterConfig<ParamTypeId, Guid?>
            .NewConfig()
            .MapWith(src => src != null ? src.Value : null);

        TypeAdapterConfig<Agency, GetAgenciesResponse>
            .NewConfig()
            .Map(d => d.AgencyTypeLibelle, s => s.AgencyType != null ? s.AgencyType.TypeDefinition.Libelle : null)
            .Map(d => d.AgencyTypeValue, s => s.AgencyType != null ? s.AgencyType.Value : null);
    }
}
