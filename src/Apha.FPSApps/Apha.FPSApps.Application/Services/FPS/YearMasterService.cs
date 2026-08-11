using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class YearMasterService : IYearMasterService
    {
        private readonly IFpsApiClient _fpsClient;

        public YearMasterService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<IEnumerable<YearMasterDto>>> GetAllFpsYearsAsync()
        {
            var yearMasters = await _fpsClient.FpsYearMaster.GetAllFpsYearsAsync();
            return yearMasters;
        }

        public async Task<ApiResponseDto<List<YearMasterDto>>> GetAllFpsYearsPagedAsync(QueryParameters<int> query)
        {
            var yearMasters = await _fpsClient.FpsYearMaster.GetAllFpsYearsPagedAsync(query);
            return yearMasters;
        }

        public async Task<ApiResponseDto<YearMasterDto>> GetFpsYearByIdAsync(int fpsYear)
        {
            var yearMaster = await _fpsClient.FpsYearMaster.GetFpsYearByIdAsync(fpsYear);
            return yearMaster;
        }

        public async Task<ApiResponseDto<int>> GetFpsPlannedYearAsync()
        {

            var yearMasters = await _fpsClient.FpsYearMaster.GetAllFpsYearsAsync();

            if(yearMasters.Success && yearMasters.Data != null)
            {
                var plannedFpsYears = yearMasters.Data
                    .Where(y => y.Active && y.YearStatus.ToLower() == "planned")
                    .OrderByDescending(y => y.FpsYear).ToList();
                
                var plannedYear = plannedFpsYears.FirstOrDefault()?.FpsYear;

                var openFpsYears = yearMasters.Data
                    .Where(y => y.Active && y.YearStatus.ToLower() == "open")
                    .OrderByDescending(y => y.FpsYear)
                    .Select(y => y.FpsYear)
                    .First(); 

                if (plannedFpsYears !=null && plannedYear.HasValue)
                {
                    return new ApiResponseDto<int> { Success=true, Data = plannedYear.Value };
                }
                else
                {
                    return new ApiResponseDto<int> { Success = true, Data = openFpsYears + 1 };
                }

            }

            return new ApiResponseDto<int>{ Success = false, Errors = yearMasters.Errors, Meta = yearMasters.Meta};
        }
    }
}
