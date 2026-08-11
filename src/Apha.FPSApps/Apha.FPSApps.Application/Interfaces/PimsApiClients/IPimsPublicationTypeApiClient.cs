using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // Mirrors PublicationTypeController — string PK (type); full CRUD; route api/v1/publication-types
    public interface IPimsPublicationTypeApiClient
    {
        // GET /api/v1/publication-types — full list
        Task<ApiResponseDto<List<PublicationTypeDto>>> GetAllPublicationTypesAsync();

        // GET /api/v1/publication-types/paged — paged/sorted/filterable list
        Task<ApiResponseDto<PaginatedResult<PublicationTypeDto>>> GetPagedPublicationTypesAsync(QueryParameters<string> query);

        // GET /api/v1/publication-types/{type}
        Task<ApiResponseDto<PublicationTypeDto>> GetPublicationTypeByCodeAsync(string type);

        // POST /api/v1/publication-types
        Task<ApiResponseDto<PublicationTypeDto>> CreatePublicationTypeAsync(PublicationTypeDto dto);

        // PUT /api/v1/publication-types/{type}
        Task<ApiResponseDto<PublicationTypeDto>> UpdatePublicationTypeAsync(string type, PublicationTypeDto dto);

        // DELETE /api/v1/publication-types/{type}
        Task<ApiResponseDto<bool>> DeletePublicationTypeAsync(string type);
    }
}
