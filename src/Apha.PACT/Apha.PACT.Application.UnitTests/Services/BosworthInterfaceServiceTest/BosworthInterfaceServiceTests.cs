using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Services;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.BosworthInterfaceServiceTest
{
    public class BosworthInterfaceServiceTests
    {
        private readonly IBosworthInterfaceRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly BosworthInterfaceService _sut;

        public BosworthInterfaceServiceTests()
        {
            _mockRepository = Substitute.For<IBosworthInterfaceRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new BosworthInterfaceService(_mockRepository, _mockMapper);
        }

        #region GetTimePurchaseProjectAsync

        [Fact]
        public async Task GetTimePurchaseProjectAsync_ValidProject_ReturnsMappedResult()
        {
            var entities = new List<TimePurchaseProject> { new() { Project = "PRJ1" } };
            var dtos = new List<TimePurchaseProjectDto> { new() { Project = "PRJ1" } };

            _mockRepository.GetTimePurchaseProjectAsync("PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimePurchaseProjectDto>>(entities).Returns(dtos);

            var result = await _sut.GetTimePurchaseProjectAsync("PRJ1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetTimePurchaseProjectAsync("PRJ1");
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_NullProject_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTimePurchaseProjectAsync(null!));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_EmptyProject_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTimePurchaseProjectAsync(""));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_WhitespaceProject_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTimePurchaseProjectAsync("   "));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetTimePurchaseProjectAsync("PRJ1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetTimePurchaseProjectAsync("PRJ1"));
        }

        #endregion

        #region GetTimeSaleProfitCentreAsync

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_ValidProfitCentre_ReturnsMappedResult()
        {
            var entities = new List<TimeSaleProfitCentre> { new() { ProfitCentre = "PC1" } };
            var dtos = new List<TimeSaleProfitCentreDto> { new() { ProfitCentre = "PC1" } };

            _mockRepository.GetTimeSaleProfitCentreAsync("PC1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeSaleProfitCentreDto>>(entities).Returns(dtos);

            var result = await _sut.GetTimeSaleProfitCentreAsync("PC1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetTimeSaleProfitCentreAsync("PC1");
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_NullProfitCentre_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTimeSaleProfitCentreAsync(null!));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROFIT_CENTRE_REQUIRED");
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_EmptyProfitCentre_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTimeSaleProfitCentreAsync(""));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROFIT_CENTRE_REQUIRED");
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_WhitespaceProfitCentre_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTimeSaleProfitCentreAsync("   "));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROFIT_CENTRE_REQUIRED");
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetTimeSaleProfitCentreAsync("PC1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetTimeSaleProfitCentreAsync("PC1"));
        }

        #endregion

        #region GetTimeSaleWorkGroupAsync

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_ValidWorkGroup_ReturnsMappedResult()
        {
            var entities = new List<TimeSaleWorkGroup> { new() { SellingWg = "WG1" } };
            var dtos = new List<TimeSaleWorkGroupDto> { new() { SellingWg = "WG1" } };

            _mockRepository.GetTimeSaleWorkGroupAsync("WG1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeSaleWorkGroupDto>>(entities).Returns(dtos);

            var result = await _sut.GetTimeSaleWorkGroupAsync("WG1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetTimeSaleWorkGroupAsync("WG1");
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_NullWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTimeSaleWorkGroupAsync(null!));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUP_REQUIRED");
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_EmptyWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTimeSaleWorkGroupAsync(""));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUP_REQUIRED");
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_WhitespaceWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTimeSaleWorkGroupAsync("   "));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUP_REQUIRED");
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetTimeSaleWorkGroupAsync("WG1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetTimeSaleWorkGroupAsync("WG1"));
        }

        #endregion

        #region GetTestSaleSellingWorkgroupAsync

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_ValidWorkGroup_ReturnsMappedResult()
        {
            var entities = new List<TestSaleSellingWorkgroup> { new() { SellerWG = "WG1" } };
            var dtos = new List<TestSaleSellingWorkgroupDto> { new() { SellerWG = "WG1" } };

            _mockRepository.GetTestSaleSellingWorkgroupAsync("WG1").Returns(entities);
            _mockMapper.Map<IEnumerable<TestSaleSellingWorkgroupDto>>(entities).Returns(dtos);

            var result = await _sut.GetTestSaleSellingWorkgroupAsync("WG1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetTestSaleSellingWorkgroupAsync("WG1");
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_NullWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTestSaleSellingWorkgroupAsync(null!));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUP_REQUIRED");
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_EmptyWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTestSaleSellingWorkgroupAsync(""));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUP_REQUIRED");
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_WhitespaceWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTestSaleSellingWorkgroupAsync("   "));

            ex.Errors.Should().ContainSingle(e => e.Code == "WORKGROUP_REQUIRED");
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetTestSaleSellingWorkgroupAsync("WG1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetTestSaleSellingWorkgroupAsync("WG1"));
        }

        #endregion

        #region GetTestSaleBuyingProjectAsync

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_ValidParentProject_ReturnsMappedResult()
        {
            var entities = new List<TestSaleBuyingProject> { new() { Buyer = "PRJ1" } };
            var dtos = new List<TestSaleBuyingProjectDto> { new() { Buyer = "PRJ1" } };

            _mockRepository.GetTestSaleBuyingProjectAsync("PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<TestSaleBuyingProjectDto>>(entities).Returns(dtos);

            var result = await _sut.GetTestSaleBuyingProjectAsync("PRJ1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetTestSaleBuyingProjectAsync("PRJ1");
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_NullParentProject_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTestSaleBuyingProjectAsync(null!));

            ex.Errors.Should().ContainSingle(e => e.Code == "PARENT_PROJECT_REQUIRED");
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_EmptyParentProject_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTestSaleBuyingProjectAsync(""));

            ex.Errors.Should().ContainSingle(e => e.Code == "PARENT_PROJECT_REQUIRED");
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_WhitespaceParentProject_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetTestSaleBuyingProjectAsync("   "));

            ex.Errors.Should().ContainSingle(e => e.Code == "PARENT_PROJECT_REQUIRED");
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetTestSaleBuyingProjectAsync("PRJ1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetTestSaleBuyingProjectAsync("PRJ1"));
        }

        #endregion
    }
}
