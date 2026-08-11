using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IPublicationTypeRepository
    {
        Task<List<PublicationType>> GetAllPublicationTypesAsync();

        Task<PagedData<PublicationType>> GetPagedPublicationTypesAsync(PaginationParameters<string> query);

        Task<PublicationType?> GetPublicationTypeByCodeAsync(string type);

        Task<PublicationType> AddPublicationTypeAsync(PublicationType entity);

        Task<PublicationType> UpdatePublicationTypeAsync(PublicationType entity);

        Task<bool> DeletePublicationTypeAsync(string type);

        Task<bool> PublicationTypeExistsAsync(string type);
    }
}
