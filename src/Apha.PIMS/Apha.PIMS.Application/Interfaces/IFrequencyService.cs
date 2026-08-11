using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IFrequencyService
    {
        Task<List<FrequencyDto>> GetAllFrequenciesAsync();

        Task<PaginatedResult<FrequencyDto>> GetPagedFrequenciesAsync(QueryParameters<string> query);

        Task<FrequencyDto?> GetFrequencyByIdAsync(int frequencyId);

        Task<FrequencyDto> CreateFrequencyAsync(FrequencyDto dto);

        Task<FrequencyDto> UpdateFrequencyAsync(FrequencyDto dto);

        Task<bool> DeleteFrequencyAsync(int frequencyId);

        Task<bool> FrequencyExistsAsync(int frequencyId);
    }
}
