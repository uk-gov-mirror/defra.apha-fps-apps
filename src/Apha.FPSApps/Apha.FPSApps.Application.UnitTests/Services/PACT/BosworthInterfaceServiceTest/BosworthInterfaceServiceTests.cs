using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.BosworthInterfaceServiceTest
{
    public class BosworthInterfaceServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactBosworthInterfaceApiClient _apiClient;
        private readonly BosworthInterfaceService _service;

        public BosworthInterfaceServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _apiClient = Substitute.For<IPactBosworthInterfaceApiClient>();
            _pactClient.PactBosworthInterface.Returns(_apiClient);
            _service = new BosworthInterfaceService(_pactClient);
        }

        #region GetTimePurchaseProjectAsync

        [Fact]
        public async Task GetTimePurchaseProjectAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TimePurchaseProjectDto>>.SuccessResponse(
                [new TimePurchaseProjectDto { Project = "P1", SellingWg = "WG1" }]);
            _apiClient.GetTimePurchaseProjectAsync("P1").Returns(expected);

            var result = await _service.GetTimePurchaseProjectAsync("P1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTimePurchaseProjectAsync("P1");
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            var expected = ApiResponseDto<List<TimePurchaseProjectDto>>.FailureResponse(
                [new ApiErrorDto { Code = "ERR", Message = "Error" }], new ApiMetaDto());
            _apiClient.GetTimePurchaseProjectAsync("P1").Returns(expected);

            var result = await _service.GetTimePurchaseProjectAsync("P1");

            Assert.False(result.Success);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            var expected = ApiResponseDto<List<TimePurchaseProjectDto>>.SuccessResponse([]);
            _apiClient.GetTimePurchaseProjectAsync("P1").Returns(expected);

            var result = await _service.GetTimePurchaseProjectAsync("P1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTimeSaleProfitCentreAsync

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TimeSaleProfitCentreDto>>.SuccessResponse(
                [new TimeSaleProfitCentreDto { ProfitCentre = "PC1", WorkGroup = "WG1" }]);
            _apiClient.GetTimeSaleProfitCentreAsync("PC1").Returns(expected);

            var result = await _service.GetTimeSaleProfitCentreAsync("PC1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTimeSaleProfitCentreAsync("PC1");
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            var expected = ApiResponseDto<List<TimeSaleProfitCentreDto>>.FailureResponse(
                [new ApiErrorDto { Code = "ERR", Message = "Error" }], new ApiMetaDto());
            _apiClient.GetTimeSaleProfitCentreAsync("PC1").Returns(expected);

            var result = await _service.GetTimeSaleProfitCentreAsync("PC1");

            Assert.False(result.Success);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            var expected = ApiResponseDto<List<TimeSaleProfitCentreDto>>.SuccessResponse([]);
            _apiClient.GetTimeSaleProfitCentreAsync("PC1").Returns(expected);

            var result = await _service.GetTimeSaleProfitCentreAsync("PC1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTimeSaleWorkGroupAsync

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TimeSaleWorkGroupDto>>.SuccessResponse(
                [new TimeSaleWorkGroupDto { SellingWg = "WG1", Project = "P1" }]);
            _apiClient.GetTimeSaleWorkGroupAsync("WG1").Returns(expected);

            var result = await _service.GetTimeSaleWorkGroupAsync("WG1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTimeSaleWorkGroupAsync("WG1");
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            var expected = ApiResponseDto<List<TimeSaleWorkGroupDto>>.FailureResponse(
                [new ApiErrorDto { Code = "ERR", Message = "Error" }], new ApiMetaDto());
            _apiClient.GetTimeSaleWorkGroupAsync("WG1").Returns(expected);

            var result = await _service.GetTimeSaleWorkGroupAsync("WG1");

            Assert.False(result.Success);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            var expected = ApiResponseDto<List<TimeSaleWorkGroupDto>>.SuccessResponse([]);
            _apiClient.GetTimeSaleWorkGroupAsync("WG1").Returns(expected);

            var result = await _service.GetTimeSaleWorkGroupAsync("WG1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTestSaleSellingWorkgroupAsync

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.SuccessResponse(
                [new TestSaleSellingWorkgroupDto { SellerWG = "WG1", TestCode = "TC1" }]);
            _apiClient.GetTestSaleSellingWorkgroupAsync("WG1").Returns(expected);

            var result = await _service.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestSaleSellingWorkgroupAsync("WG1");
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            var expected = ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.FailureResponse(
                [new ApiErrorDto { Code = "ERR", Message = "Error" }], new ApiMetaDto());
            _apiClient.GetTestSaleSellingWorkgroupAsync("WG1").Returns(expected);

            var result = await _service.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.False(result.Success);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            var expected = ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.SuccessResponse([]);
            _apiClient.GetTestSaleSellingWorkgroupAsync("WG1").Returns(expected);

            var result = await _service.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTestSaleBuyingProjectAsync

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TestSaleBuyingProjectDto>>.SuccessResponse(
                [new TestSaleBuyingProjectDto { Buyer = "B1", TestCode = "TC1" }]);
            _apiClient.GetTestSaleBuyingProjectAsync("PP1").Returns(expected);

            var result = await _service.GetTestSaleBuyingProjectAsync("PP1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestSaleBuyingProjectAsync("PP1");
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_WhenApiReturnsFailure_ReturnsFailure()
        {
            var expected = ApiResponseDto<List<TestSaleBuyingProjectDto>>.FailureResponse(
                [new ApiErrorDto { Code = "ERR", Message = "Error" }], new ApiMetaDto());
            _apiClient.GetTestSaleBuyingProjectAsync("PP1").Returns(expected);

            var result = await _service.GetTestSaleBuyingProjectAsync("PP1");

            Assert.False(result.Success);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            var expected = ApiResponseDto<List<TestSaleBuyingProjectDto>>.SuccessResponse([]);
            _apiClient.GetTestSaleBuyingProjectAsync("PP1").Returns(expected);

            var result = await _service.GetTestSaleBuyingProjectAsync("PP1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        // ── DTO Property Tests ─────────────────────────────────────────────────

        #region TimePurchaseProjectDto

        [Fact]
        public void TimePurchaseProjectDto_Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var dto = new TimePurchaseProjectDto
            {
                Project   = "PRJ1",
                SellingWg = "WG1",
                GradeCode = "G1",
                Name      = "Alice",
                Time      = 10.5,
                Cost      = 250.75,
                Month     = 3,
                JobCode   = "JC1"
            };

            Assert.Equal("PRJ1",  dto.Project);
            Assert.Equal("WG1",   dto.SellingWg);
            Assert.Equal("G1",    dto.GradeCode);
            Assert.Equal("Alice", dto.Name);
            Assert.Equal(10.5,    dto.Time);
            Assert.Equal(250.75,  dto.Cost);
            Assert.Equal(3,       dto.Month);
            Assert.Equal("JC1",   dto.JobCode);
        }

        [Fact]
        public void TimePurchaseProjectDto_NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new TimePurchaseProjectDto
            {
                Project   = null,
                SellingWg = null,
                GradeCode = null,
                Name      = null,
                Time      = null,
                Cost      = null,
                JobCode   = null
            };

            Assert.Null(dto.Project);
            Assert.Null(dto.SellingWg);
            Assert.Null(dto.GradeCode);
            Assert.Null(dto.Name);
            Assert.Null(dto.Time);
            Assert.Null(dto.Cost);
            Assert.Null(dto.JobCode);
        }

        [Fact]
        public void TimePurchaseProjectDto_DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new TimePurchaseProjectDto();

            Assert.Null(dto.Project);
            Assert.Null(dto.SellingWg);
            Assert.Null(dto.GradeCode);
            Assert.Null(dto.Name);
            Assert.Null(dto.Time);
            Assert.Null(dto.Cost);
            Assert.Equal(0, dto.Month);
            Assert.Null(dto.JobCode);
        }

        [Fact]
        public void TimePurchaseProjectDto_Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new TimePurchaseProjectDto { Project = "P1" };

            dto.Project   = "P2";
            dto.SellingWg = "WG2";
            dto.GradeCode = "G2";
            dto.Name      = "Bob";
            dto.Time      = 20.0;
            dto.Cost      = 500.0;
            dto.Month     = 6;
            dto.JobCode   = "JC2";

            Assert.Equal("P2",   dto.Project);
            Assert.Equal("WG2",  dto.SellingWg);
            Assert.Equal("G2",   dto.GradeCode);
            Assert.Equal("Bob",  dto.Name);
            Assert.Equal(20.0,   dto.Time);
            Assert.Equal(500.0,  dto.Cost);
            Assert.Equal(6,      dto.Month);
            Assert.Equal("JC2",  dto.JobCode);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-100.5)]
        [InlineData(999999.99)]
        public void TimePurchaseProjectDto_Time_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new TimePurchaseProjectDto { Time = value };

            Assert.Equal(value, dto.Time);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-500.25)]
        [InlineData(9999999.99)]
        public void TimePurchaseProjectDto_Cost_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new TimePurchaseProjectDto { Cost = value };

            Assert.Equal(value, dto.Cost);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(12.0)]
        public void TimePurchaseProjectDto_Month_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new TimePurchaseProjectDto { Month = value };

            Assert.Equal(value, dto.Month);
        }

        #endregion

        #region TimeSaleProfitCentreDto

        [Fact]
        public void TimeSaleProfitCentreDto_Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var dto = new TimeSaleProfitCentreDto
            {
                ProfitCentre  = "PC1",
                WorkGroup     = "WG1",
                GradeCode     = "G1",
                Name          = "Alice",
                ParentProject = "PP1",
                JobCode       = "JC1",
                SumOfTime     = 100.5,
                SumOfCost     = 5000.75
            };

            Assert.Equal("PC1",   dto.ProfitCentre);
            Assert.Equal("WG1",   dto.WorkGroup);
            Assert.Equal("G1",    dto.GradeCode);
            Assert.Equal("Alice", dto.Name);
            Assert.Equal("PP1",   dto.ParentProject);
            Assert.Equal("JC1",   dto.JobCode);
            Assert.Equal(100.5,   dto.SumOfTime);
            Assert.Equal(5000.75, dto.SumOfCost);
        }

        [Fact]
        public void TimeSaleProfitCentreDto_NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new TimeSaleProfitCentreDto
            {
                ProfitCentre  = null,
                WorkGroup     = null,
                GradeCode     = null,
                Name          = null,
                ParentProject = null,
                JobCode       = null,
                SumOfTime     = null,
                SumOfCost     = null
            };

            Assert.Null(dto.ProfitCentre);
            Assert.Null(dto.WorkGroup);
            Assert.Null(dto.GradeCode);
            Assert.Null(dto.Name);
            Assert.Null(dto.ParentProject);
            Assert.Null(dto.JobCode);
            Assert.Null(dto.SumOfTime);
            Assert.Null(dto.SumOfCost);
        }

        [Fact]
        public void TimeSaleProfitCentreDto_DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new TimeSaleProfitCentreDto();

            Assert.Null(dto.ProfitCentre);
            Assert.Null(dto.WorkGroup);
            Assert.Null(dto.GradeCode);
            Assert.Null(dto.Name);
            Assert.Null(dto.ParentProject);
            Assert.Null(dto.JobCode);
            Assert.Null(dto.SumOfTime);
            Assert.Null(dto.SumOfCost);
        }

        [Fact]
        public void TimeSaleProfitCentreDto_Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new TimeSaleProfitCentreDto { ProfitCentre = "PC1" };

            dto.ProfitCentre  = "PC2";
            dto.WorkGroup     = "WG2";
            dto.GradeCode     = "G2";
            dto.Name          = "Bob";
            dto.ParentProject = "PP2";
            dto.JobCode       = "JC2";
            dto.SumOfTime     = 200.0;
            dto.SumOfCost     = 10000.0;

            Assert.Equal("PC2",    dto.ProfitCentre);
            Assert.Equal("WG2",    dto.WorkGroup);
            Assert.Equal("G2",     dto.GradeCode);
            Assert.Equal("Bob",    dto.Name);
            Assert.Equal("PP2",    dto.ParentProject);
            Assert.Equal("JC2",    dto.JobCode);
            Assert.Equal(200.0,    dto.SumOfTime);
            Assert.Equal(10000.0,  dto.SumOfCost);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-50.25)]
        [InlineData(999999.99)]
        public void TimeSaleProfitCentreDto_SumOfTime_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new TimeSaleProfitCentreDto { SumOfTime = value };

            Assert.Equal(value, dto.SumOfTime);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1000.50)]
        [InlineData(9999999.99)]
        public void TimeSaleProfitCentreDto_SumOfCost_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new TimeSaleProfitCentreDto { SumOfCost = value };

            Assert.Equal(value, dto.SumOfCost);
        }

        #endregion

        #region TestSaleSellingWorkgroupDto

        [Fact]
        public void TestSaleSellingWorkgroupDto_Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var dto = new TestSaleSellingWorkgroupDto
            {
                ProgramNo = "PG1",
                BuyerType = "External",
                Buyer     = "B1",
                SellerWG  = "WG1",
                Portfolio = "PF1",
                TestCode  = "TC1",
                Month     = 4.0,
                Volume    = 25.5,
                Fee       = 1500.75m
            };

            Assert.Equal("PG1",      dto.ProgramNo);
            Assert.Equal("External", dto.BuyerType);
            Assert.Equal("B1",       dto.Buyer);
            Assert.Equal("WG1",      dto.SellerWG);
            Assert.Equal("PF1",      dto.Portfolio);
            Assert.Equal("TC1",      dto.TestCode);
            Assert.Equal(4.0,        dto.Month);
            Assert.Equal(25.5,       dto.Volume);
            Assert.Equal(1500.75m,   dto.Fee);
        }

        [Fact]
        public void TestSaleSellingWorkgroupDto_NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new TestSaleSellingWorkgroupDto
            {
                ProgramNo = null,
                BuyerType = null,
                Buyer     = null,
                SellerWG  = null,
                Portfolio = null,
                TestCode  = null,
                Month     = null,
                Volume    = null,
                Fee       = null
            };

            Assert.Null(dto.ProgramNo);
            Assert.Null(dto.BuyerType);
            Assert.Null(dto.Buyer);
            Assert.Null(dto.SellerWG);
            Assert.Null(dto.Portfolio);
            Assert.Null(dto.TestCode);
            Assert.Null(dto.Month);
            Assert.Null(dto.Volume);
            Assert.Null(dto.Fee);
        }

        [Fact]
        public void TestSaleSellingWorkgroupDto_DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new TestSaleSellingWorkgroupDto();

            Assert.Null(dto.ProgramNo);
            Assert.Null(dto.BuyerType);
            Assert.Null(dto.Buyer);
            Assert.Null(dto.SellerWG);
            Assert.Null(dto.Portfolio);
            Assert.Null(dto.TestCode);
            Assert.Null(dto.Month);
            Assert.Null(dto.Volume);
            Assert.Null(dto.Fee);
        }

        [Fact]
        public void TestSaleSellingWorkgroupDto_Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new TestSaleSellingWorkgroupDto { SellerWG = "WG1" };

            dto.ProgramNo = "PG2";
            dto.BuyerType = "Internal";
            dto.Buyer     = "B2";
            dto.SellerWG  = "WG2";
            dto.Portfolio = "PF2";
            dto.TestCode  = "TC2";
            dto.Month     = 8.0;
            dto.Volume    = 50.0;
            dto.Fee       = 3000.00m;

            Assert.Equal("PG2",      dto.ProgramNo);
            Assert.Equal("Internal", dto.BuyerType);
            Assert.Equal("B2",       dto.Buyer);
            Assert.Equal("WG2",      dto.SellerWG);
            Assert.Equal("PF2",      dto.Portfolio);
            Assert.Equal("TC2",      dto.TestCode);
            Assert.Equal(8.0,        dto.Month);
            Assert.Equal(50.0,       dto.Volume);
            Assert.Equal(3000.00m,   dto.Fee);
        }

        [Theory]
        [InlineData(0.00)]
        [InlineData(-250.50)]
        [InlineData(9999999.99)]
        public void TestSaleSellingWorkgroupDto_Fee_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto = new TestSaleSellingWorkgroupDto { Fee = value };

            Assert.Equal(value, dto.Fee);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-10.5)]
        [InlineData(100000.0)]
        public void TestSaleSellingWorkgroupDto_Volume_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new TestSaleSellingWorkgroupDto { Volume = value };

            Assert.Equal(value, dto.Volume);
        }

        #endregion

        #region TestSaleBuyingProjectDto

        [Fact]
        public void TestSaleBuyingProjectDto_Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var dto = new TestSaleBuyingProjectDto
            {
                ProgramNo = "PG1",
                Buyer     = "B1",
                SellerPC  = "PC1",
                SellerWG  = "WG1",
                TestCode  = "TC1",
                Month     = 6.0,
                Volume    = 15.5,
                Charge    = 2000.50m
            };

            Assert.Equal("PG1",    dto.ProgramNo);
            Assert.Equal("B1",     dto.Buyer);
            Assert.Equal("PC1",    dto.SellerPC);
            Assert.Equal("WG1",    dto.SellerWG);
            Assert.Equal("TC1",    dto.TestCode);
            Assert.Equal(6.0,      dto.Month);
            Assert.Equal(15.5,     dto.Volume);
            Assert.Equal(2000.50m, dto.Charge);
        }

        [Fact]
        public void TestSaleBuyingProjectDto_NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new TestSaleBuyingProjectDto
            {
                ProgramNo = null,
                Buyer     = null,
                SellerPC  = null,
                SellerWG  = null,
                TestCode  = null,
                Month     = null,
                Volume    = null,
                Charge    = null
            };

            Assert.Null(dto.ProgramNo);
            Assert.Null(dto.Buyer);
            Assert.Null(dto.SellerPC);
            Assert.Null(dto.SellerWG);
            Assert.Null(dto.TestCode);
            Assert.Null(dto.Month);
            Assert.Null(dto.Volume);
            Assert.Null(dto.Charge);
        }

        [Fact]
        public void TestSaleBuyingProjectDto_DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new TestSaleBuyingProjectDto();

            Assert.Null(dto.ProgramNo);
            Assert.Null(dto.Buyer);
            Assert.Null(dto.SellerPC);
            Assert.Null(dto.SellerWG);
            Assert.Null(dto.TestCode);
            Assert.Null(dto.Month);
            Assert.Null(dto.Volume);
            Assert.Null(dto.Charge);
        }

        [Fact]
        public void TestSaleBuyingProjectDto_Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new TestSaleBuyingProjectDto { Buyer = "B1" };

            dto.ProgramNo = "PG2";
            dto.Buyer     = "B2";
            dto.SellerPC  = "PC2";
            dto.SellerWG  = "WG2";
            dto.TestCode  = "TC2";
            dto.Month     = 12.0;
            dto.Volume    = 30.0;
            dto.Charge    = 4000.00m;

            Assert.Equal("PG2",    dto.ProgramNo);
            Assert.Equal("B2",     dto.Buyer);
            Assert.Equal("PC2",    dto.SellerPC);
            Assert.Equal("WG2",    dto.SellerWG);
            Assert.Equal("TC2",    dto.TestCode);
            Assert.Equal(12.0,     dto.Month);
            Assert.Equal(30.0,     dto.Volume);
            Assert.Equal(4000.00m, dto.Charge);
        }

        [Theory]
        [InlineData(0.00)]
        [InlineData(-500.25)]
        [InlineData(9999999.99)]
        public void TestSaleBuyingProjectDto_Charge_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto = new TestSaleBuyingProjectDto { Charge = value };

            Assert.Equal(value, dto.Charge);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-5.5)]
        [InlineData(100000.0)]
        public void TestSaleBuyingProjectDto_Volume_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new TestSaleBuyingProjectDto { Volume = value };

            Assert.Equal(value, dto.Volume);
        }

        #endregion
    }
}
