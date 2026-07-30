using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class YearlyFinancialDataService : IYearlyFinancialDataService
    {
      
        private readonly IPimsApiClient _client;

        public YearlyFinancialDataService(IPimsApiClient client)
        {
            _client = client;
        }
        
        public async Task<ApiResponseDto<List<YearlyFinancialDataDto>>> GetAllAsync(string project, QueryParameters<string> query)
            => await _client.PimsYearlyFinancialData.GetAllAsync(project, query);

        public async Task<ApiResponseDto<YearlyFinancialDataDto>> GetByKeyAsync(short year, string project)
            => await _client.PimsYearlyFinancialData.GetByKeyAsync(year, project);

        
        public async Task<ApiResponseDto<YearlyFinancialDataDto>> CreateAsync(YearlyFinancialDataDto dto)
            => await _client.PimsYearlyFinancialData.CreateAsync(dto);

        
        public async Task<ApiResponseDto<YearlyFinancialDataDto>> UpdateAsync(short year, string project, YearlyFinancialDataDto dto)
            => await _client.PimsYearlyFinancialData.UpdateAsync(year, project, dto);

       
        public async Task<ApiResponseDto<object>> DeleteAsync(short year, string project)
            => await _client.PimsYearlyFinancialData.DeleteAsync(year, project);

                        
        public async Task<ApiResponseDto<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year)
            => await _client.PimsYearlyFinancialData.GetPactCostsAsync(project, year);

        
        public async Task<ApiResponseDto<string>> GetSettingValueByIdAsync(string id)
            => await _client.PimsYearlyFinancialData.GetSettingValueByIdAsync(id);
    }
}
