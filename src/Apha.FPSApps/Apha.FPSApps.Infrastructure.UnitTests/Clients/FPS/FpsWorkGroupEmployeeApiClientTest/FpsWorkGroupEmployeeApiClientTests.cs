using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsWorkGroupEmployeeApiClientTest
{
    public class FpsWorkGroupEmployeeApiClientTests
    {
        private const string DefaultWgGrade = "WG01";
        private const string DefaultPactId  = "PACT001";

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsWorkGroupEmployeeApiClient _client;

        public FpsWorkGroupEmployeeApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsWorkGroupEmployeeApiClient(_http, _mapper);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithSuccessResponse_ReturnsMappedEmployeeList()
        {
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<WorkGroupEmployeeRes>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var apiResponse = new ApiResponse<List<WorkGroupEmployeeRes>> { Success = true, Data = resList };
            var dtoList     = new List<WorkGroupEmployeeDto>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var expectedDto = ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(dtoList);

            _http.GetAsync<List<WorkGroupEmployeeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithSuccessResponse_ReturnsMappedLegacyEmployee()
        {
            var res         = new WorkGroupEmployeeRes { PactId = DefaultPactId, SpNumber = "SP001" };
            var apiResponse = new ApiResponse<WorkGroupEmployeeRes> { Success = true, Data = res };
            var dto         = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var expectedDto = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(dto);

            _http.GetAsync<WorkGroupEmployeeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(DefaultPactId, result.Data?.PactId);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeByIdForStaffAsync_WithSuccessResponse_ReturnsMappedStaffEmployee()
        {
            var res         = new WorkGroupEmployeeRes { PactId = DefaultPactId, SpNumber = "SP001" };
            var apiResponse = new ApiResponse<WorkGroupEmployeeRes> { Success = true, Data = res };
            var dto         = new WorkGroupEmployeeStaffDto { PactId = DefaultPactId };
            var expectedDto = ApiResponseDto<WorkGroupEmployeeStaffDto>.SuccessResponse(dto);

            _http.GetAsync<WorkGroupEmployeeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetWorkGroupEmployeeByIdForStaffAsync(DefaultPactId);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(DefaultPactId, result.Data?.PactId);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_WithFailureResponse_ReturnsFailureDto()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WorkGroupEmployeeRes>> { Success = false };
            var expectedDto = ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse([], new ApiMetaDto());

            _http.GetAsync<List<WorkGroupEmployeeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetWorkGroupEmployeeForStaffAsync(query, DefaultWgGrade);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateWorkGroupEmployeeForStaffAsync_WithFailureResponse_ReturnsFailureDto()
        {
            var dto = new WorkGroupEmployeeStaffDto { PactId = DefaultPactId };
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId };
            var apiResponse = new ApiResponse<WorkGroupEmployeeRes> { Success = false };
            var expectedDto = ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse([], new ApiMetaDto());

            _mapper.Map<WorkGroupEmployeeReq>(dto).Returns(req);
            _http.PostAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.CreateWorkGroupEmployeeForStaffAsync(dto);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithSuccessResponse_ReturnsMappedLegacyEmployee()
        {
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId, HrsPaid = 40.0 };
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId, HrsPaid = 40.0 };
            var res = new WorkGroupEmployeeRes { PactId = DefaultPactId };
            var apiResponse = new ApiResponse<WorkGroupEmployeeRes> { Success = true, Data = res };
            var updatedDto  = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var expectedDto = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(updatedDto);

            _mapper.Map<WorkGroupEmployeeReq>(dto).Returns(req);
            _http.PutAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.UpdateWorkGroupEmployeeAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeForStaffAsync_WithFailureResponse_ReturnsFailureDto()
        {
            var dto = new WorkGroupEmployeeStaffDto { PactId = DefaultPactId };
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId };
            var apiResponse = new ApiResponse<WorkGroupEmployeeRes> { Success = false };
            var expectedDto = ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse([], new ApiMetaDto());

            _mapper.Map<WorkGroupEmployeeReq>(dto).Returns(req);
            _http.PutAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.UpdateWorkGroupEmployeeForStaffAsync(dto);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_WithSuccessResponse_ReturnsMappedActiveEmployeeList()
        {
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<WorkGroupEmployeeRes>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var apiResponse = new ApiResponse<List<WorkGroupEmployeeRes>> { Success = true, Data = resList };
            var dtoList     = new List<WorkGroupEmployeeStaffDto>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var expectedDto = ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.SuccessResponse(dtoList);

            _http.GetAsync<List<WorkGroupEmployeeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetAllActiveWorkGroupEmployeesAsync(query, DefaultWgGrade);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_WithFailureResponse_ReturnsFailureDto()
        {
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WorkGroupEmployeeRes>> { Success = false };
            var expectedDto = ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse([], new ApiMetaDto());

            _http.GetAsync<List<WorkGroupEmployeeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetAllActiveWorkGroupEmployeesAsync(query, DefaultWgGrade);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithSuccessResponse_ReturnsSuccess()
        {
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            var result = await _client.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithFailureResponse_ReturnsFailureDto()
        {
            var apiResponse = new ApiResponse<bool> { Success = false };
            var expectedDto = ApiResponseDto<bool>.FailureResponse([], new ApiMetaDto());

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            var result = await _client.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }
    }
}
