using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IPublicationTypeService
    {
        Task<List<PublicationTypeDto>> GetAllPublicationTypesAsync();

        Task<PaginatedResult<PublicationTypeDto>> GetPagedPublicationTypesAsync(QueryParameters<string> query);

        Task<PublicationTypeDto?> GetPublicationTypeByCodeAsync(string type);

        Task<PublicationTypeDto> CreatePublicationTypeAsync(PublicationTypeDto dto);

        Task<PublicationTypeDto> UpdatePublicationTypeAsync(PublicationTypeDto dto);

        Task<bool> DeletePublicationTypeAsync(string type);

        Task<bool> PublicationTypeExistsAsync(string type);
    }
}
