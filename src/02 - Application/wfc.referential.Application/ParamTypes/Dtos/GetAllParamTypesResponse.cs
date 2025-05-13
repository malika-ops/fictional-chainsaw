using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Dtos;

public record GetAllParamTypesResponse(
    Guid ParamTypeId,
    string Value,
    bool IsEnabled,
    TypeDefinitionId TypeDefinitionId
);