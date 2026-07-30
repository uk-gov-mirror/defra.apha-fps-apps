using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsYearlyFinancialDataApiClientTest
{
    public class PimsYearlyFinancialDataApiClientTests
    {
        private readonly IPimsHttpExecutor             _http;
        private readonly IMapper                       _mapper;
        private readonly PimsYearlyFinancialDataApiClient _client;

        public PimsYearlyFinancialDataApiClientTests()
        {
            _http   = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsYearlyFinancialDataApiClient(_http, _mapper);
        }

        // ── helpers ──────────────────────────────────────────────────────

        private const string BaseUrl = "api/v1/yearlyfinancialdata";

        private static ApiResponse<T> OkResponse<T>(T data)
            => new() { Success = true, Data = data };

        private static ApiResponse<T> FailResponse<T>()
            => new() { Success = false, Errors = [new ApiError { Message = "err", Code = "ERR" }] };

        private static ApiResponseDto<T> OkDto<T>(T data)
            => new() { Success = true, Data = data };

        private static ApiResponseDto<T> FailDto<T>()
            => new() { Success = false, Errors = [new ApiErrorDto { Message = "err", Code = "ERR" }], Meta = new ApiMetaDto() };

        private static QueryParameters<string> DefaultQuery()
            => new() { Page = 1, PageSize = 10 };

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesClient()
        {
            var client = new PimsYearlyFinancialDataApiClient(_http, _mapper);
            Assert.NotNull(client);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query       = DefaultQuery();
            var resList     = new List<YearlyFinancialDataRes> { new() { Year = 2024, Project = "PP001" } };
            var apiResponse = OkResponse(resList);
            var mappedDto   = OkDto(new List<YearlyFinancialDataDto> { new() { Year = 2024, Project = "PP001" } });

            _http.GetAsync<List<YearlyFinancialDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearlyFinancialDataDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllAsync("PP001", query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            _mapper.Received(1).Map<ApiResponseDto<List<YearlyFinancialDataDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllAsync_UrlContainsProject()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = OkResponse(new List<YearlyFinancialDataRes>());
            var mappedDto   = OkDto(new List<YearlyFinancialDataDto>());
            _http.GetAsync<List<YearlyFinancialDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearlyFinancialDataDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllAsync("PP001", query);

            // Assert
            await _http.Received(1).GetAsync<List<YearlyFinancialDataRes>>(
                Arg.Is<string>(u => u.Contains($"{BaseUrl}/PP001")));
        }

        [Fact]
        public async Task GetAllAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = FailResponse<List<YearlyFinancialDataRes>>();
            var failDto     = FailDto<List<YearlyFinancialDataDto>>();

            _http.GetAsync<List<YearlyFinancialDataRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearlyFinancialDataDto>>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetAllAsync("PP001", query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllAsync_HttpThrowsException_ReturnsInternalError()
        {
            // Arrange
            var query = DefaultQuery();
            _http.GetAsync<List<YearlyFinancialDataRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllAsync("PP001", query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        #region GetByKeyAsync Tests

        [Fact]
        public async Task GetByKeyAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var res         = new YearlyFinancialDataRes { Year = 2024, Project = "PP001" };
            var apiResponse = OkResponse(res);
            var mappedDto   = OkDto(new YearlyFinancialDataDto { Year = 2024, Project = "PP001" });

            _http.GetAsync<YearlyFinancialDataRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetByKeyAsync((short)2024, "PP001");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            _mapper.Received(1).Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse);
        }

        [Fact]
        public async Task GetByKeyAsync_UrlContainsYearAndProject()
        {
            // Arrange
            var apiResponse = OkResponse(new YearlyFinancialDataRes());
            var mappedDto   = OkDto(new YearlyFinancialDataDto());
            _http.GetAsync<YearlyFinancialDataRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetByKeyAsync((short)2024, "PP001");

            // Assert
            await _http.Received(1).GetAsync<YearlyFinancialDataRes>(
                Arg.Is<string>(u => u.Contains("2024") && u.Contains("PP001")));
        }

        [Fact]
        public async Task GetByKeyAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailResponse<YearlyFinancialDataRes>();
            var failDto     = FailDto<YearlyFinancialDataDto>();
            _http.GetAsync<YearlyFinancialDataRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetByKeyAsync((short)9999, "UNKNOWN");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetByKeyAsync_HttpThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<YearlyFinancialDataRes>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Timeout"));

            // Act
            var result = await _client.GetByKeyAsync((short)2024, "PP001");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var dto         = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            var req         = new YearlyFinancialDataReq();
            var apiResponse = OkResponse(new YearlyFinancialDataRes { Year = 2024, Project = "PP001" });
            var mappedDto   = OkDto(dto);

            
            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(req);
            _http.PostAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<YearlyFinancialDataReq>(dto);
            _mapper.Received(1).Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse);
        }

        [Fact]
        public async Task CreateAsync_MapsDtoToReqBeforePosting()
        {
            // Arrange
            var dto = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            var req = new YearlyFinancialDataReq();
            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(req);
            _http.PostAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), req)
                 .Returns(OkResponse(new YearlyFinancialDataRes()));
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(Arg.Any<ApiResponse<YearlyFinancialDataRes>>())
                   .Returns(OkDto(dto));

            // Act
            await _client.CreateAsync(dto);

            // Assert: mapper called for Dto→Req
            _mapper.Received(1).Map<YearlyFinancialDataReq>(dto);
            // Assert: PostAsync called with the mapped Req
            await _http.Received(1).PostAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(BaseUrl, req);
        }

        [Fact]
        public async Task CreateAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            var req = new YearlyFinancialDataReq();
            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(req);
            var apiResponse = FailResponse<YearlyFinancialDataRes>();
            var failDto     = FailDto<YearlyFinancialDataDto>();
            _http.PostAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAsync_HttpThrowsException_ReturnsInternalError()
        {
            // Arrange
            var dto = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(new YearlyFinancialDataReq());
            _http.PostAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), Arg.Any<YearlyFinancialDataReq>())
                 .ThrowsAsync(new Exception("Server error"));

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("INTERNAL_ERROR", result.Errors![0].Code);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var dto         = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            var req         = new YearlyFinancialDataReq();
            var apiResponse = OkResponse(new YearlyFinancialDataRes { Year = 2024, Project = "PP001" });
            var mappedDto   = OkDto(dto);

           
            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(req);
            _http.PutAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateAsync((short)2024, "PP001", dto);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<YearlyFinancialDataReq>(dto);
            _mapper.Received(1).Map<ApiResponseDto<YearlyFinancialDataDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateAsync_UrlContainsYearAndProject()
        {
            // Arrange
            var dto = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            var req = new YearlyFinancialDataReq();
            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(req);
            _http.PutAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), req)
                 .Returns(OkResponse(new YearlyFinancialDataRes()));
            _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(Arg.Any<ApiResponse<YearlyFinancialDataRes>>())
                   .Returns(OkDto(dto));

            // Act
            await _client.UpdateAsync((short)2024, "PP001", dto);

            // Assert
            await _http.Received(1).PutAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(
                Arg.Is<string>(u => u.Contains("2024") && u.Contains("PP001")), req);
        }

        [Fact]
        public async Task UpdateAsync_HttpThrowsException_ReturnsInternalError()
        {
            // Arrange
            var dto = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            _mapper.Map<YearlyFinancialDataReq>(dto).Returns(new YearlyFinancialDataReq());
            _http.PutAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(Arg.Any<string>(), Arg.Any<YearlyFinancialDataReq>())
                 .ThrowsAsync(new Exception("Put failed"));

            // Act
            var result = await _client.UpdateAsync((short)2024, "PP001", dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("INTERNAL_ERROR", result.Errors![0].Code);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange — client calls DeleteAsync<object>, not bool
            var apiResponse = OkResponse(new object());
            var mappedDto   = new ApiResponseDto<object> { Success = true, Data = new object() };

            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteAsync((short)2024, "PP001");

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<object>>(apiResponse);
        }

        [Fact]
        public async Task DeleteAsync_UrlContainsYearAndProject()
        {
            // Arrange — client calls DeleteAsync<object>
            var apiResponse = OkResponse(new object());
            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(new ApiResponseDto<object> { Success = true });

            // Act
            await _client.DeleteAsync((short)2024, "PP001");

            // Assert
            await _http.Received(1).DeleteAsync<object>(
                Arg.Is<string>(u => u.Contains("2024") && u.Contains("PP001")));
        }

        [Fact]
        public async Task DeleteAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange — client calls DeleteAsync<object>
            var apiResponse = FailResponse<object>();
            var failDto     = new ApiResponseDto<object>
            {
                Success = false,
                Errors  = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }],
                Meta    = new ApiMetaDto()
            };
            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.DeleteAsync((short)9999, "UNKNOWN");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_HttpThrowsException_ReturnsInternalError()
        {
            // Arrange — client calls DeleteAsync<object>
            _http.DeleteAsync<object>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Delete failed"));

            // Act
            var result = await _client.DeleteAsync((short)2024, "PP001");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        #region GetPactCostsAsync Tests


        [Fact]
        public async Task GetPactCostsAsync_HttpReturnsSuccessWithData_ReturnsAggregatedYearTotals()
        {
            // Arrange
            var resList = new List<PactProjectYearCostsRes>
            {
                new()
                {
                    Project = "PP001",
                    Year = 2024,
                    SubContracts = 900m,
                    Animals = 100m,
                    Tests = 400m,
                    Pay = 700m,
                    NonPayOH = 200m,
                    TotalCosts = 2500m,
                    TimeCost = 900m,
                    Hours = 22,
                    CustIncome = 300m,
                    BudgetCvl = 4000m
                },
                new()
                {
                    Project = "PP001",
                    Year = 2024,
                    SubContracts = 100m,
                    Animals = 20m,
                    Tests = 50m,
                    Pay = 30m,
                    NonPayOH = 10m,
                    TotalCosts = 90m,
                    TimeCost = 40m,
                    Hours = 7,
                    CustIncome = 300m,
                    BudgetCvl = 4000m
                }
            };
            var apiResponse = OkResponse(resList);

            _http.GetAsync<List<PactProjectYearCostsRes>>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetPactCostsAsync("PP001", (short)2024);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("PP001", result.Data.Project);
            Assert.Equal((short)2024, result.Data.Year);
            Assert.Equal(1000m, result.Data.SubContracts);
            Assert.Equal(120m, result.Data.Animals);
            Assert.Equal(450m, result.Data.Tests);
            Assert.Equal(730m, result.Data.Pay);
            Assert.Equal(210m, result.Data.NonPayOH);
            Assert.Equal(2590m, result.Data.TotalCosts);
            Assert.Equal(940m, result.Data.TimeCost);
            Assert.Equal(29d, result.Data.Hours);
            Assert.Equal(300m, result.Data.CustIncome);
            Assert.Equal(4000m, result.Data.BudgetCvl);
            _mapper.DidNotReceive().Map<PactProjectYearCostsDto>(Arg.Any<PactProjectYearCostsRes>());
        }

        [Fact]
        public async Task GetPactCostsAsync_HttpReturnsSuccessWithEmptyList_ReturnsEmptyDtoForRequestedKey()
        {
            // Arrange
            var apiResponse = OkResponse(new List<PactProjectYearCostsRes>());
            _http.GetAsync<List<PactProjectYearCostsRes>>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetPactCostsAsync("PP001", (short)2024);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("PP001", result.Data.Project);
            Assert.Equal((short)2024, result.Data.Year);
            _mapper.DidNotReceive().Map<PactProjectYearCostsDto>(Arg.Any<PactProjectYearCostsRes>());
        }

        [Fact]
        public async Task GetPactCostsAsync_UrlContainsProjectAndYearAndPactCostsSuffix()
        {
            // Arrange
            var apiResponse = OkResponse(new List<PactProjectYearCostsRes>());
            _http.GetAsync<List<PactProjectYearCostsRes>>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.GetPactCostsAsync("PP001", (short)2024);

            // Assert
            await _http.Received(1).GetAsync<List<PactProjectYearCostsRes>>(
                Arg.Is<string>(u =>
                    u.Contains("PP001") &&
                    u.Contains("2024") &&
                    u.Contains("pactcosts")));
        }

        [Fact]
        public async Task GetPactCostsAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailResponse<List<PactProjectYearCostsRes>>();
            var failDto     = new ApiResponseDto<PactProjectYearCostsDto>
            {
                Success = false,
                Errors  = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }],
                Meta    = new ApiMetaDto()
            };
            _http.GetAsync<List<PactProjectYearCostsRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PactProjectYearCostsDto>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetPactCostsAsync("PP001", (short)2024);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPactCostsAsync_HttpThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<PactProjectYearCostsRes>>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Timeout"));

            // Act
            var result = await _client.GetPactCostsAsync("PP001", (short)2024);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        #region GetSettingValueByIdAsync Tests


        [Fact]
        public async Task GetSettingValueByIdAsync_HttpReturnsSuccessWithData_ReturnsSettingValue()
        {
            // Arrange
            var apiResponse = OkResponse("7.4");
            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetSettingValueByIdAsync("HoursInDay");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("7.4", result.Data);
            // Mapper must NOT be called for the success path
            _mapper.DidNotReceive().Map<ApiResponseDto<string>>(Arg.Any<ApiResponse<string>>());
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_HttpReturnsSuccessWithNullData_ReturnsFailureResponse()
        {
            // Arrange — Success == true but Data is null; client falls through to failure branch
            var apiResponse = new ApiResponse<string> { Success = true, Data = null };
            var failDto     = FailDto<string>();
            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetSettingValueByIdAsync("HoursInDay");

            // Assert
            Assert.False(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<string>>(apiResponse);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_UrlContainsSettingId()
        {
            // Arrange
            var apiResponse = OkResponse("220");
            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.GetSettingValueByIdAsync("DaysInYear");

            // Assert
            await _http.Received(1).GetAsync<string>(
                Arg.Is<string>(u => u.Contains("DaysInYear")));
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailResponse<string>();
            var failDto     = FailDto<string>();
            _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<string>>(apiResponse).Returns(failDto);

            // Act
            var result = await _client.GetSettingValueByIdAsync("HoursInDay");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            _mapper.Received(1).Map<ApiResponseDto<string>>(apiResponse);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_HttpThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<string>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetSettingValueByIdAsync("HoursInDay");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion
    }
}
