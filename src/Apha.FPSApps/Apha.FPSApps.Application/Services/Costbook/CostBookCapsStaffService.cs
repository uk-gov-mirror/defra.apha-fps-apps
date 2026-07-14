using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Services.Costbook
{
    
    public class CostBookCapsStaffService : ICostBookCapsStaffService
    {
        
        private readonly ICostBookApiClient _costBookClient;

        public CostBookCapsStaffService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }
       
        public Task<ApiResponseDto<List<StaffDto>>> GetPaginatedCapsStaffAsync(QueryParameters<string> query)
        {
            return _costBookClient.CostbookCapsStaff.GetPaginatedCapsStaffAsync(query);
        }

        public Task<ApiResponseDto<StaffDto>> GetCapsStaffByMNumberAsync(string mNumber)
        {
            return _costBookClient.CostbookCapsStaff.GetCapsStaffByMNumberAsync(mNumber);
        }

        public Task<ApiResponseDto<StaffDto>> AddCapsStaffAsync(StaffDto dto)
        {
            return _costBookClient.CostbookCapsStaff.AddCapsStaffAsync(dto);
        }

        public Task<ApiResponseDto<StaffDto>> UpdateCapsStaffAsync(string mNumber, StaffDto dto)
        {
            return _costBookClient.CostbookCapsStaff.UpdateCapsStaffAsync(mNumber, dto);
        }

        public Task<ApiResponseDto<bool>> DeleteCapsStaffAsync(string mNumber)
        {
            return _costBookClient.CostbookCapsStaff.DeleteCapsStaffAsync(mNumber);
        }
    }
}
