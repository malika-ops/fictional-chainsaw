using wfc.referential.Application.Services.Dtos;

namespace wfc.referential.Application.Products.Dtos;

public record GetAllProductsResponse(
    Guid ProductId,
    string Code,
    string Name,
    bool IsEnabled,
    List<GetAllServicesResponse>? Services
    );
