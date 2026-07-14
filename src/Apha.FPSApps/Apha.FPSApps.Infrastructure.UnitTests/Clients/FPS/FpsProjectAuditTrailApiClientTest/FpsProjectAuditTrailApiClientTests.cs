using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProjectAuditTrailApiClientTest
{
    public class FpsProjectAuditTrailApiClientTests
    {
        private const string TestProject = "PROJ001";

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProjectAuditTrailApiClient _client;

        public FpsProjectAuditTrailApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProjectAuditTrailApiClient(_http, _mapper);
        }

        // ── GetProjectLogsAsync ──────────────────────────────────────────────

        #region GetProjectLogsAsync

        [Fact]
        public async Task GetProjectLogsAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<ProjectLogRes>>
            {
                Success = true,
                Data = new List<ProjectLogRes> { new() },
                Pagination = new Pagination { TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<ProjectLogDto>>.SuccessResponse(
                new List<ProjectLogDto> { new() }, new PaginationDto { TotalRecords = 1 });

            _http.GetAsync<List<ProjectLogRes>>(
                    Arg.Is<string>(url => url.Contains("projectaudittrail/projectlogs") && url.Contains($"project={TestProject}")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectLogDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectLogDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetProjectLogsAsync_HttpReturnsFailure_ReturnsMappedFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<ProjectLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedFailure = new ApiResponseDto<List<ProjectLogDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectLogRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectLogDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetProjectLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetProjectLogsAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<ProjectLogRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetProjectLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithDateRange_IncludesDateParamsInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var fromDate = new DateOnly(2024, 1, 1);
            var toDate = new DateOnly(2024, 12, 31);
            var httpResponse = new ApiResponse<List<ProjectLogRes>> { Success = true, Data = new List<ProjectLogRes>() };
            var expectedDto = ApiResponseDto<List<ProjectLogDto>>.SuccessResponse(new List<ProjectLogDto>());

            _http.GetAsync<List<ProjectLogRes>>(
                    Arg.Is<string>(url =>
                        url.Contains("fromDate=2024-01-01") &&
                        url.Contains("toDate=2024-12-31")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ProjectLogDto>>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectLogsAsync(query, TestProject, fromDate, toDate);

            // Assert — URL must include both date params
            await _http.Received(1).GetAsync<List<ProjectLogRes>>(
                Arg.Is<string>(url =>
                    url.Contains("fromDate=2024-01-01") &&
                    url.Contains("toDate=2024-12-31")));
        }

        #endregion

        // ── GetStaffJobLogsAsync ─────────────────────────────────────────────

        #region GetStaffJobLogsAsync

        [Fact]
        public async Task GetStaffJobLogsAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<StaffJobLogRes>>
            {
                Success = true,
                Data = new List<StaffJobLogRes> { new() },
                Pagination = new Pagination { TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<StaffJobLogDto>>.SuccessResponse(
                new List<StaffJobLogDto> { new() });

            _http.GetAsync<List<StaffJobLogRes>>(
                    Arg.Is<string>(url => url.Contains("projectaudittrail/staffjoblogs") && url.Contains($"project={TestProject}")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<StaffJobLogDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetStaffJobLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<StaffJobLogDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<StaffJobLogRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Timeout"));

            // Act
            var result = await _client.GetStaffJobLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<StaffJobLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "ERROR" } }
            };
            var mappedFailure = new ApiResponseDto<List<StaffJobLogDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<StaffJobLogRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<StaffJobLogDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetStaffJobLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetTestRequirementLogsAsync ──────────────────────────────────────

        #region GetTestRequirementLogsAsync

        [Fact]
        public async Task GetTestRequirementLogsAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<TestRequirementLogRes>>
            {
                Success = true,
                Data = new List<TestRequirementLogRes> { new() },
                Pagination = new Pagination { TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<TestRequirementLogDto>>.SuccessResponse(
                new List<TestRequirementLogDto> { new() });

            _http.GetAsync<List<TestRequirementLogRes>>(
                    Arg.Is<string>(url => url.Contains("projectaudittrail/testrequirementlogs") && url.Contains($"project={TestProject}")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementLogDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTestRequirementLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<TestRequirementLogDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<TestRequirementLogRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Connection refused"));

            // Act
            var result = await _client.GetTestRequirementLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<TestRequirementLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "ERROR" } }
            };
            var mappedFailure = new ApiResponseDto<List<TestRequirementLogDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestRequirementLogRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementLogDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetTestRequirementLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetAnimalRequestLogsAsync ────────────────────────────────────────

        #region GetAnimalRequestLogsAsync

        [Fact]
        public async Task GetAnimalRequestLogsAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<AnimalRequestLogRes>>
            {
                Success = true,
                Data = new List<AnimalRequestLogRes> { new() },
                Pagination = new Pagination { TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<AnimalRequestLogDto>>.SuccessResponse(
                new List<AnimalRequestLogDto> { new() });

            _http.GetAsync<List<AnimalRequestLogRes>>(
                    Arg.Is<string>(url => url.Contains("projectaudittrail/animalrequestlogs") && url.Contains($"project={TestProject}")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<AnimalRequestLogDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAnimalRequestLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<AnimalRequestLogDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<AnimalRequestLogRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Service unavailable"));

            // Act
            var result = await _client.GetAnimalRequestLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<AnimalRequestLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "ERROR" } }
            };
            var mappedFailure = new ApiResponseDto<List<AnimalRequestLogDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AnimalRequestLogRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<AnimalRequestLogDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetAnimalRequestLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetAdditionalCostLogsAsync ───────────────────────────────────────

        #region GetAdditionalCostLogsAsync

        [Fact]
        public async Task GetAdditionalCostLogsAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<AdditionalCostLogRes>>
            {
                Success = true,
                Data = new List<AdditionalCostLogRes> { new() },
                Pagination = new Pagination { TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<AdditionalCostLogDto>>.SuccessResponse(
                new List<AdditionalCostLogDto> { new() });

            _http.GetAsync<List<AdditionalCostLogRes>>(
                    Arg.Is<string>(url => url.Contains("projectaudittrail/additionalcostlogs") && url.Contains($"project={TestProject}")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostLogDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAdditionalCostLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<AdditionalCostLogDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<AdditionalCostLogRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network timeout"));

            // Act
            var result = await _client.GetAdditionalCostLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<AdditionalCostLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "ERROR" } }
            };
            var mappedFailure = new ApiResponseDto<List<AdditionalCostLogDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AdditionalCostLogRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostLogDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetAdditionalCostLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithDateRange_IncludesDateParamsInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var fromDate = new DateOnly(2024, 3, 1);
            var toDate = new DateOnly(2024, 3, 31);
            var httpResponse = new ApiResponse<List<AdditionalCostLogRes>> { Success = true, Data = new List<AdditionalCostLogRes>() };
            var expectedDto = ApiResponseDto<List<AdditionalCostLogDto>>.SuccessResponse(new List<AdditionalCostLogDto>());

            _http.GetAsync<List<AdditionalCostLogRes>>(
                    Arg.Is<string>(url =>
                        url.Contains("fromDate=2024-03-01") &&
                        url.Contains("toDate=2024-03-31")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostLogDto>>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.GetAdditionalCostLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            await _http.Received(1).GetAsync<List<AdditionalCostLogRes>>(
                Arg.Is<string>(url =>
                    url.Contains("fromDate=2024-03-01") &&
                    url.Contains("toDate=2024-03-31")));
        }

        #endregion
    }
}
