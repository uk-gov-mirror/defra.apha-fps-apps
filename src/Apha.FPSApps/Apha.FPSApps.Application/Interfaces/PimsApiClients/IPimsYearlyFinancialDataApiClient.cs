using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{

    public interface IPimsYearlyFinancialDataApiClient
    {
       
        Task<ApiResponseDto<List<YearlyFinancialDataDto>>> GetAllAsync(string project, QueryParameters<string> query);

      
        Task<ApiResponseDto<YearlyFinancialDataDto>> GetByKeyAsync(short year, string project);

      
        Task<ApiResponseDto<YearlyFinancialDataDto>> CreateAsync(YearlyFinancialDataDto dto);

      
        Task<ApiResponseDto<YearlyFinancialDataDto>> UpdateAsync(short year, string project, YearlyFinancialDataDto dto);

        
        Task<ApiResponseDto<object>> DeleteAsync(short year, string project);

       
        Task<ApiResponseDto<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year);

        Task<ApiResponseDto<string>> GetSettingValueByIdAsync(string id);
    }
}
