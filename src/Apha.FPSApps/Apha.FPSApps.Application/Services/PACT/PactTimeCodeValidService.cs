using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class PactTimeCodeValidService : IPactTimeCodeValidService
    {
        private readonly IPactApiClient _pactClient;

        public PactTimeCodeValidService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetByJobCodeAsync(string jobCode, string parentProject)
            => await _pactClient.PactTimeCodeValid.GetByJobCodeAsync(jobCode, parentProject);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetTimeCodeValidsByWorkGroupAsync(string workGroup)
        {
            //check if workGroup is not null or empty
            if (string.IsNullOrEmpty(workGroup))
            {
                throw new ArgumentException("WorkGroup cannot be null or empty", nameof(workGroup));
            }
            return await _pactClient.PactTimeCodeValid.GetTimeCodeValidsByWorkGroupAsync(workGroup);
        }

        public async Task<ApiResponseDto<List<string>>> GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync(string workGroup, string timeCode)
        {
            //check if workGroup and timeCode are not null or empty
            if (string.IsNullOrEmpty(workGroup))
            {
                throw new ArgumentException("WorkGroup cannot be null or empty", nameof(workGroup));
            }

            if (string.IsNullOrEmpty(timeCode))
            {
                throw new ArgumentException("TimeCode cannot be null or empty", nameof(timeCode));
            }

            return await _pactClient.PactTimeCodeValid.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync(workGroup, timeCode);
        }

        public async Task<ApiResponseDto<List<string>>> GetAllDistinctTimeCodesAsync()
            => await _pactClient.PactTimeCodeValid.GetAllDistinctTimeCodesAsync();

        public async Task<ApiResponseDto<List<string>>> GetAllDistinctProjectsAsync()
            => await _pactClient.PactTimeCodeValid.GetAllDistinctProjectsAsync();

        public async Task<ApiResponseDto<TimeCodeValidDto>> GetTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
            => await _pactClient.PactTimeCodeValid.GetTimeCodeValidAsync(workGroup, timeCode, parentProject);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedTimeCodesAsync(QueryParameters<string> query, string? jobCode, string? parentProject)
            => await _pactClient.PactTimeCodeValid.GetPagedTimeCodesAsync(query, jobCode, parentProject);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedByProjectAndTestCodeAsync(QueryParameters<string> query, string parentProject, string testCode)
            => await _pactClient.PactTimeCodeValid.GetPagedByProjectAndTestCodeAsync(query, parentProject, testCode);

        public async Task<ApiResponseDto<TimeCodeValidDto>> CreateTimeCodeValidAsync(TimeCodeValidDto item)
            => await _pactClient.PactTimeCodeValid.CreateTimeCodeValidAsync(item);

        public async Task<ApiResponseDto<TimeCodeValidDto>> UpdateTimeCodeValidAsync(TimeCodeValidDto item)
            => await _pactClient.PactTimeCodeValid.UpdateTimeCodeValidAsync(item);

        public async Task<ApiResponseDto<bool>> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
            => await _pactClient.PactTimeCodeValid.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);

        public async Task<ApiResponseDto<bool>> DeleteAllByJobCodeAsync(string jobCode, string parentProject)
            => await _pactClient.PactTimeCodeValid.DeleteAllByJobCodeAsync(jobCode, parentProject);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> CopyWorkGroupAsync(string sourceJobCode, string targetJobCode, string parentProject)
            => await _pactClient.PactTimeCodeValid.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);

        public async Task<ApiResponseDto<bool>> DeleteBulkAsync(BulkDeleteTimeCodeRequestDto request)
            => await _pactClient.PactTimeCodeValid.DeleteBulkAsync(request);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> CopySelectedWorkGroupsAsync(BulkCopyWorkGroupRequestDto request)
            => await _pactClient.PactTimeCodeValid.CopySelectedWorkGroupsAsync(request);
    }
}
