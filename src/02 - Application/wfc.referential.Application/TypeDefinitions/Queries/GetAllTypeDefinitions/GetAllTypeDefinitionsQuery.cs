using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.TypeDefinitions.Dtos;

namespace wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions
{
    public record GetAllTypeDefinitionsQuery : IQuery<PagedResult<GetAllTypeDefinitionsResponse>>
    {
        public int PageNumber { get; }
        public int PageSize { get; }
        public string? Libelle { get; init; }
        public string? Description { get; init; }
        public bool? IsEnabled { get; init; }

        public GetAllTypeDefinitionsQuery(int pageNumber, int pageSize, string? libelle, string? description, bool? isEnabled = true)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            Libelle = libelle;
            Description = description;
            IsEnabled = isEnabled;
        }
    }
}