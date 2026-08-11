using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IFrequencyRepository
    {
        Task<List<Frequency>> GetAllFrequenciesAsync();

        Task<PagedData<Frequency>> GetPagedFrequenciesAsync(PaginationParameters<string> query);

        Task<Frequency?> GetFrequencyByIdAsync(int frequencyId);

        Task<Frequency> AddFrequencyAsync(Frequency entity);

        Task<Frequency> UpdateFrequencyAsync(Frequency entity);

        Task<bool> DeleteFrequencyAsync(int frequencyId);

        Task<bool> FrequencyExistsAsync(int frequencyId);
    }
}
