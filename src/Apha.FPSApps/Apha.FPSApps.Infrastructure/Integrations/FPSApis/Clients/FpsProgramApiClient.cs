using Apha.Common.Constants;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Net.Http.Headers;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProgramApiClient : IFpsProgramApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string internalCodeError = "INTERNAL_ERROR";

        public FpsProgramApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<IEnumerable<ProgramDto>>> GetAllProgramsAsync()
        {
            var response = await _http.GetAsync<IEnumerable<ProgramDto>>(FpsApiEndpoints.GetAllPrograms);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<IEnumerable<ProgramDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<ProgramDto>>>(response);
                return ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }

        }

        public async Task<ApiResponseDto<IEnumerable<ProgramDto>>> GetAllProgramsForAllUsersAsync()
        {
            var response = await _http.GetAsync<IEnumerable<ProgramDto>>(FpsApiEndpoints.GetAllProgramsForAllUsers);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<IEnumerable<ProgramDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<ProgramDto>>>(response);
                return ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<ProgramDto>>> GetAllProgramsAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedPrograms, query);
            var response = await _http.GetAsync<List<ProgramDto>>(url);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<ProgramDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<ProgramDto>>>(response);
                return ApiResponseDto<List<ProgramDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<ProgramPlanCostViewDto>>> GetProgramTimeSnapshotAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetProgramTimeSnapshot, query);
            var response = await _http.GetAsync<List<ProgramPlanCostViewDto>>(url);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<ProgramPlanCostViewDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<ProgramPlanCostViewDto>>>(response);
                return ApiResponseDto<List<ProgramPlanCostViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<ProgramDto?>> GetProgramByIdAsync(string programNo)
        {
            var response = await _http.GetAsync<ProgramDto>(string.Format(FpsApiEndpoints.GetProgramById, programNo));
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<ProgramDto?>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<ProgramDto?>>(response);
                return ApiResponseDto<ProgramDto?>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<ProgramDto>> AddProgramAsync(ProgramDto programDto)
        {
            var response = await _http.PostAsync<ProgramDto, ProgramDto>(FpsApiEndpoints.CreateProgram, programDto);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<ProgramDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<ProgramDto>>(response);
                return ApiResponseDto<ProgramDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<ProgramDto>> UpdateProgramAsync(ProgramDto programDto)
        {
            var response = await _http.PutAsync<ProgramDto, ProgramDto>(FpsApiEndpoints.UpdateProgram, programDto);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<ProgramDto>>(response);
            }
            else
            {
                var reponseDto = _mapper.Map<ApiResponseDto<ProgramDto>>(response);
                return ApiResponseDto<ProgramDto>.FailureResponse(reponseDto.Errors, reponseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteProgramAsync(string programNo)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteProgram, programNo));
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<bool>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }
    }
}