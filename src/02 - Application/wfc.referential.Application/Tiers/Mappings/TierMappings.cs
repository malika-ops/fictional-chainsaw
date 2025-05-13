using Mapster;
using wfc.referential.Application.Tiers.Commands.CreateTier;
using wfc.referential.Application.Tiers.Commands.DeleteTier;
using wfc.referential.Application.Tiers.Commands.PatchTier;
using wfc.referential.Application.Tiers.Commands.UpdateTier;
using wfc.referential.Application.Tiers.Dtos;
using wfc.referential.Application.Tiers.Queries.GetAllTiers;
using wfc.referential.Domain.TierAggregate;

namespace wfc.referential.Application.Tiers.Mappings;

public class TierMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CreateTierRequest, CreateTierCommand>.NewConfig();

        TypeAdapterConfig<UpdateTierRequest, UpdateTierCommand>.NewConfig();

        TypeAdapterConfig<PatchTierRequest, PatchTierCommand>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<PatchTierCommand, Tier>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<DeleteTierRequest, DeleteTierCommand>.NewConfig();

        TypeAdapterConfig<GetAllTiersRequest, GetAllTiersQuery>
            .NewConfig()
            .Map(q => q.PageNumber, r => r.PageNumber)
            .Map(q => q.PageSize, r => r.PageSize);

        TypeAdapterConfig<Tier, TierResponse>
            .NewConfig()
            .Map(dest => dest.TierId, src => src.Id.Value);



    }
}
