using Mapster;
using wfc.referential.Application.Agencies.Commands.CreateAgency;
using wfc.referential.Application.Agencies.Commands.DeleteAgency;
using wfc.referential.Application.Agencies.Commands.PatchAgency;
using wfc.referential.Application.Agencies.Commands.UpdateAgency;
using wfc.referential.Application.Agencies.Dtos;
using wfc.referential.Application.Agencies.Queries.GetAllAgencies;
using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Application.Agencies.Mappings;

public class AgencyMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CreateAgencyRequest, CreateAgencyCommand>.NewConfig();
        TypeAdapterConfig<UpdateAgencyRequest, UpdateAgencyCommand>.NewConfig();
        TypeAdapterConfig<DeleteAgencyRequest, DeleteAgencyCommand>.NewConfig();

        TypeAdapterConfig<PatchAgencyRequest, PatchAgencyCommand>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<PatchAgencyCommand, Agency>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<GetAllAgenciesRequest, GetAllAgenciesQuery>
            .NewConfig()
            .Map(q => q.PageNumber, r => r.PageNumber)
            .Map(q => q.PageSize, r => r.PageSize);

        TypeAdapterConfig<Agency, GetAgenciesResponse>
            .NewConfig()
            .Map(d => d.AgencyId, s => s.Id.Value)
            .Map(d => d.CityId, s => s.CityId == null ? (Guid?)null : s.CityId.Value)
            .Map(d => d.SectorId, s => s.SectorId == null ? (Guid?)null : s.SectorId.Value)
            .Map(d => d.AgencyTypeId, s => s.AgencyTypeId == null ? (Guid?)null : s.AgencyTypeId.Value)
            .Map(d => d.AgencyTypeLibelle, s => s.AgencyType == null ? null : s.AgencyType.TypeDefinition.Libelle)
            .Map(d => d.AgencyTypeValue, s => s.AgencyType == null ? null : s.AgencyType.Value);
    }
}
