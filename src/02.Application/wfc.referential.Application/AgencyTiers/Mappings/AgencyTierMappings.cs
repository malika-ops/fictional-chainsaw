using Mapster;
using wfc.referential.Application.AgencyTiers.Commands.CreateAgencyTier;
using wfc.referential.Application.AgencyTiers.Commands.DeleteAgencyTier;
using wfc.referential.Application.AgencyTiers.Commands.PatchAgencyTier;
using wfc.referential.Application.AgencyTiers.Commands.UpdateAgencyTier;
using wfc.referential.Application.AgencyTiers.Dtos;
using wfc.referential.Application.AgencyTiers.Queries.GetAllAgencyTiers;
using wfc.referential.Domain.AgencyTierAggregate;

namespace wfc.referential.Application.AgencyTiers.Mappings;

public class AgencyTierMappings
{
    public static void Register()
    {


        TypeAdapterConfig<CreateAgencyTierRequest, CreateAgencyTierCommand>.NewConfig();
        TypeAdapterConfig<UpdateAgencyTierRequest, UpdateAgencyTierCommand>.NewConfig();
        TypeAdapterConfig<DeleteAgencyTierRequest, DeleteAgencyTierCommand>.NewConfig();


        TypeAdapterConfig<PatchAgencyTierRequest, PatchAgencyTierCommand>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<PatchAgencyTierCommand, AgencyTier>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<GetAllAgencyTiersRequest, GetAllAgencyTiersQuery>
            .NewConfig()
            .Map(q => q.PageNumber, r => r.PageNumber)
            .Map(q => q.PageSize, r => r.PageSize);


        TypeAdapterConfig<AgencyTier, AgencyTierResponse>
            .NewConfig()
            .Map(d => d.AgencyTierId, s => s.Id.Value)
            .Map(d => d.AgencyId, s => s.AgencyId.Value)
            .Map(d => d.TierId, s => s.TierId.Value);
    }
}
