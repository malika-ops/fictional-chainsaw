using Mapster;
using wfc.referential.Application.PartnerCountries.Commands.CreatePartnerCountry;
using wfc.referential.Application.PartnerCountries.Commands.DeletePartnerCountry;
using wfc.referential.Application.PartnerCountries.Commands.UpdatePartnerCountry;
using wfc.referential.Application.PartnerCountries.Dtos;
using wfc.referential.Application.PartnerCountries.Queries.GetAllPartnerCountries;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;

namespace wfc.referential.Application.PartnerCountries.Mappings;

public class PartnerCountryMappings
{
    public static void Register()
    {
        TypeAdapterConfig<CreatePartnerCountryRequest, CreatePartnerCountryCommand>
            .NewConfig();

        TypeAdapterConfig<UpdatePartnerCountryRequest, UpdatePartnerCountryCommand>
            .NewConfig();    

        TypeAdapterConfig<DeletePartnerCountryRequest, DeletePartnerCountryCommand>
            .NewConfig()
            .MapToConstructor(true);  

        TypeAdapterConfig<GetAllPartnerCountriesRequest, GetAllPartnerCountriesQuery>
            .NewConfig()
            .Map(dest => dest.PageNumber, src => src.PageNumber)
            .Map(dest => dest.PageSize, src => src.PageSize);

        TypeAdapterConfig<PartnerCountry, PartnerCountryResponse>
            .NewConfig()
            .Map(d => d.PartnerCountryId, s => s.Id.Value)
            .Map(d => d.PartnerId, s => s.PartnerId.Value)
            .Map(d => d.CountryId, s => s.CountryId.Value);

        TypeAdapterConfig<PartnerId, Guid>.NewConfig().MapWith(p => p.Value);
        TypeAdapterConfig<CountryId, Guid>.NewConfig().MapWith(c => c.Value);


    }
}
