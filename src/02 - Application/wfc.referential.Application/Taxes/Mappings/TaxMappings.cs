using Mapster;
using wfc.referential.Application.Taxes.Commands.PatchTax;
using wfc.referential.Application.Taxes.Dtos;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Taxes.Mappings;

public class TaxMappings
{
    public static void Register()
    {
        TypeAdapterConfig<Tax, GetAllTaxesResponse>.NewConfig()
            .Map(dest => dest.Id, src => src.Id!.Value);

        TypeAdapterConfig<PatchTaxRequest, PatchTaxCommand>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<PatchTaxCommand, Tax>
            .NewConfig()
            .IgnoreNullValues(true);

        // Map from TaxId to nullable Guid
        TypeAdapterConfig<TaxId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        // Map from nullable Guid to TaxId
        TypeAdapterConfig<Guid?, TaxId>
            .NewConfig()
            .MapWith(src => src.HasValue ? TaxId.Of(src.Value) : null);
    }

}
