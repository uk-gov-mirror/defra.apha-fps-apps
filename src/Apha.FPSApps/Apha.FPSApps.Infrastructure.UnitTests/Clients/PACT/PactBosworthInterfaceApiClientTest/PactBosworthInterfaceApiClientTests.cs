using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactBosworthInterfaceApiClientTest
{
    public class PactBosworthInterfaceApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactBosworthInterfaceApiClient _client;

        public PactBosworthInterfaceApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactBosworthInterfaceApiClient(_http, _mapper);
        }

        #region GetTimePurchaseProjectAsync

        [Fact]
        public async Task GetTimePurchaseProjectAsync_WhenApiReturnsSuccess_ReturnsMappedResponse()
        {
            var apiResponse = new ApiResponse<List<TimePurchaseProjectRes>>
            {
                Success = true,
                Data = [new TimePurchaseProjectRes { Project = "P1", SellingWg = "WG1" }]
            };
            var expectedDto = ApiResponseDto<List<TimePurchaseProjectDto>>.SuccessResponse(
                [new TimePurchaseProjectDto { Project = "P1", SellingWg = "WG1" }]);

            _http.GetAsync<List<TimePurchaseProjectRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/bosworth-interface/time-purchase-project") && url.Contains("P1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimePurchaseProjectDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTimePurchaseProjectAsync("P1");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<TimePurchaseProjectRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/bosworth-interface/time-purchase-project")));
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<TimePurchaseProjectRes>> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<List<TimePurchaseProjectDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "NOT_FOUND", Message = "Not Found" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TimePurchaseProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimePurchaseProjectDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetTimePurchaseProjectAsync("P1");

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_EncodesProjectParameter()
        {
            var apiResponse = new ApiResponse<List<TimePurchaseProjectRes>>
            {
                Success = true,
                Data = []
            };
            var expectedDto = ApiResponseDto<List<TimePurchaseProjectDto>>.SuccessResponse([]);

            _http.GetAsync<List<TimePurchaseProjectRes>>(Arg.Is<string>(url =>
                url.Contains(Uri.EscapeDataString("P&1"))))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimePurchaseProjectDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTimePurchaseProjectAsync("P&1");

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TimePurchaseProjectRes>>(Arg.Is<string>(url =>
                url.Contains(Uri.EscapeDataString("P&1"))));
        }

        #endregion

        #region GetTimeSaleProfitCentreAsync

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_WhenApiReturnsSuccess_ReturnsMappedResponse()
        {
            var apiResponse = new ApiResponse<List<TimeSaleProfitCentreRes>>
            {
                Success = true,
                Data = [new TimeSaleProfitCentreRes { ProfitCentre = "PC1", WorkGroup = "WG1" }]
            };
            var expectedDto = ApiResponseDto<List<TimeSaleProfitCentreDto>>.SuccessResponse(
                [new TimeSaleProfitCentreDto { ProfitCentre = "PC1", WorkGroup = "WG1" }]);

            _http.GetAsync<List<TimeSaleProfitCentreRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/bosworth-interface/time-sale-profit-centre") && url.Contains("PC1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeSaleProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTimeSaleProfitCentreAsync("PC1");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<TimeSaleProfitCentreRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/bosworth-interface/time-sale-profit-centre")));
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var errors = new List<ApiError> { new() { Message = "Error", Code = "SERVER_ERROR" } };
            var apiResponse = new ApiResponse<List<TimeSaleProfitCentreRes>> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<List<TimeSaleProfitCentreDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "SERVER_ERROR", Message = "Error" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TimeSaleProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeSaleProfitCentreDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetTimeSaleProfitCentreAsync("PC1");

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("SERVER_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_EncodesProfitCentreParameter()
        {
            var apiResponse = new ApiResponse<List<TimeSaleProfitCentreRes>>
            {
                Success = true,
                Data = []
            };
            var expectedDto = ApiResponseDto<List<TimeSaleProfitCentreDto>>.SuccessResponse([]);

            _http.GetAsync<List<TimeSaleProfitCentreRes>>(Arg.Is<string>(url =>
                url.Contains(Uri.EscapeDataString("PC&1"))))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeSaleProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTimeSaleProfitCentreAsync("PC&1");

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TimeSaleProfitCentreRes>>(Arg.Is<string>(url =>
                url.Contains(Uri.EscapeDataString("PC&1"))));
        }

        #endregion

        #region GetTimeSaleWorkGroupAsync

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_WhenApiReturnsSuccess_ReturnsMappedResponse()
        {
            var apiResponse = new ApiResponse<List<TimeSaleWorkGroupRes>>
            {
                Success = true,
                Data = [new TimeSaleWorkGroupRes { SellingWg = "WG1", Project = "PRJ1" }]
            };
            var expectedDto = ApiResponseDto<List<TimeSaleWorkGroupDto>>.SuccessResponse(
                [new TimeSaleWorkGroupDto { SellingWg = "WG1", Project = "PRJ1" }]);

            _http.GetAsync<List<TimeSaleWorkGroupRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/bosworth-interface/time-sale-workgroup") && url.Contains("WG1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeSaleWorkGroupDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTimeSaleWorkGroupAsync("WG1");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<TimeSaleWorkGroupRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/bosworth-interface/time-sale-workgroup")));
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var errors = new List<ApiError> { new() { Message = "Error", Code = "SERVER_ERROR" } };
            var apiResponse = new ApiResponse<List<TimeSaleWorkGroupRes>> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<List<TimeSaleWorkGroupDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "SERVER_ERROR", Message = "Error" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TimeSaleWorkGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeSaleWorkGroupDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetTimeSaleWorkGroupAsync("WG1");

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("SERVER_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_EncodesWorkGroupParameter()
        {
            var apiResponse = new ApiResponse<List<TimeSaleWorkGroupRes>>
            {
                Success = true,
                Data = []
            };
            var expectedDto = ApiResponseDto<List<TimeSaleWorkGroupDto>>.SuccessResponse([]);

            _http.GetAsync<List<TimeSaleWorkGroupRes>>(Arg.Is<string>(url =>
                url.Contains(Uri.EscapeDataString("WG&1"))))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeSaleWorkGroupDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTimeSaleWorkGroupAsync("WG&1");

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TimeSaleWorkGroupRes>>(Arg.Is<string>(url =>
                url.Contains(Uri.EscapeDataString("WG&1"))));
        }

        #endregion

        #region GetTestSaleSellingWorkgroupAsync

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_WhenApiReturnsSuccess_ReturnsMappedResponse()
        {
            var apiResponse = new ApiResponse<List<TestSaleSellingWorkgroupRes>>
            {
                Success = true,
                Data = [new TestSaleSellingWorkgroupRes { SellerWG = "WG1", TestCode = "TC1" }]
            };
            var expectedDto = ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.SuccessResponse(
                [new TestSaleSellingWorkgroupDto { SellerWG = "WG1", TestCode = "TC1" }]);

            _http.GetAsync<List<TestSaleSellingWorkgroupRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/bosworth-interface/test-sale-selling-workgroup") && url.Contains("WG1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSaleSellingWorkgroupDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<TestSaleSellingWorkgroupRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/bosworth-interface/test-sale-selling-workgroup")));
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var errors = new List<ApiError> { new() { Message = "Bad Request", Code = "BAD_REQUEST" } };
            var apiResponse = new ApiResponse<List<TestSaleSellingWorkgroupRes>> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<List<TestSaleSellingWorkgroupDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "BAD_REQUEST", Message = "Bad Request" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestSaleSellingWorkgroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSaleSellingWorkgroupDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("BAD_REQUEST", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_EncodesWorkGroupParameter()
        {
            var apiResponse = new ApiResponse<List<TestSaleSellingWorkgroupRes>>
            {
                Success = true,
                Data = []
            };
            var expectedDto = ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestSaleSellingWorkgroupRes>>(Arg.Is<string>(url =>
                url.Contains(Uri.EscapeDataString("WG&1"))))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSaleSellingWorkgroupDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTestSaleSellingWorkgroupAsync("WG&1");

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TestSaleSellingWorkgroupRes>>(Arg.Is<string>(url =>
                url.Contains(Uri.EscapeDataString("WG&1"))));
        }

        #endregion

        #region GetTestSaleBuyingProjectAsync

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_WhenApiReturnsSuccess_ReturnsMappedResponse()
        {
            var apiResponse = new ApiResponse<List<TestSaleBuyingProjectRes>>
            {
                Success = true,
                Data = [new TestSaleBuyingProjectRes { Buyer = "B1", TestCode = "TC1" }]
            };
            var expectedDto = ApiResponseDto<List<TestSaleBuyingProjectDto>>.SuccessResponse(
                [new TestSaleBuyingProjectDto { Buyer = "B1", TestCode = "TC1" }]);

            _http.GetAsync<List<TestSaleBuyingProjectRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/bosworth-interface/test-sale-buying-project") && url.Contains("PP1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSaleBuyingProjectDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTestSaleBuyingProjectAsync("PP1");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<TestSaleBuyingProjectRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/bosworth-interface/test-sale-buying-project")));
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var errors = new List<ApiError> { new() { Message = "Timeout", Code = "TIMEOUT" } };
            var apiResponse = new ApiResponse<List<TestSaleBuyingProjectRes>> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<List<TestSaleBuyingProjectDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "TIMEOUT", Message = "Timeout" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestSaleBuyingProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSaleBuyingProjectDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetTestSaleBuyingProjectAsync("PP1");

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("TIMEOUT", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_EncodesParentProjectParameter()
        {
            var apiResponse = new ApiResponse<List<TestSaleBuyingProjectRes>>
            {
                Success = true,
                Data = []
            };
            var expectedDto = ApiResponseDto<List<TestSaleBuyingProjectDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestSaleBuyingProjectRes>>(Arg.Is<string>(url =>
                url.Contains(Uri.EscapeDataString("PP&1"))))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSaleBuyingProjectDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTestSaleBuyingProjectAsync("PP&1");

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TestSaleBuyingProjectRes>>(Arg.Is<string>(url =>
                url.Contains(Uri.EscapeDataString("PP&1"))));
        }

        #endregion
    }
}
