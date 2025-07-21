using Mapster;
using wfc.referential.Application.Operators.Dtos;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Mappings;

public class OperatorMappings
{
    public static void Register()
    {
        TypeAdapterConfig<Operator, GetOperatorsResponse>
             .NewConfig()
             .Map(dest => dest.OperatorId, src => src.Id.Value);

        TypeAdapterConfig<OperatorId, Guid?>
              .NewConfig()
              .MapWith(src => src == null ? (Guid?)null : src.Value);

        TypeAdapterConfig<OperatorId, Guid>
       .NewConfig()
       .MapWith(src => src.Value);
    }
}