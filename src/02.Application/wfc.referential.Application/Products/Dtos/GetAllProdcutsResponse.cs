using wfc.referential.Application.Services.Dtos;

namespace wfc.referential.Application.Products.Dtos;

public record GetFiltredProductsResponse(
    Guid ProductId,
    string Code,
    string Name,
    bool IsEnabled,
    List<GetFiltredServicesResponse>? Services
    );
