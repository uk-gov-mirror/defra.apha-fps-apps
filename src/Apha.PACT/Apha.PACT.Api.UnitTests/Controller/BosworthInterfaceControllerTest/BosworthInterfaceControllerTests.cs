using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.BosworthInterfaceControllerTest
{
    public class BosworthInterfaceControllerTests
    {
        private readonly IBosworthInterfaceService _service;
        private readonly IMapper _mapper;
        private readonly BosworthInterfaceController _controller;

        public BosworthInterfaceControllerTests()
        {
            _service = Substitute.For<IBosworthInterfaceService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new BosworthInterfaceController(_service, _mapper);
        }

        #region GetTimePurchaseProject

        [Fact]
        public async Task GetTimePurchaseProject_HappyPath_ReturnsOkWithMappedResult()
        {
            var serviceResult = new List<TimePurchaseProjectDto> { new() { Project = "PRJ1" } };
            var mapped = new List<TimePurchaseProjectRes> { new() { Project = "PRJ1" } };

            _service.GetTimePurchaseProjectAsync("PRJ1").Returns(serviceResult);
            _mapper.Map<IEnumerable<TimePurchaseProjectRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTimePurchaseProject("PRJ1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTimePurchaseProject_ServiceThrows_PropagatesException()
        {
            _service.GetTimePurchaseProjectAsync("PRJ1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetTimePurchaseProject("PRJ1"));
        }

        [Fact]
        public async Task GetTimePurchaseProject_EmptyResult_ReturnsOk()
        {
            var serviceResult = Enumerable.Empty<TimePurchaseProjectDto>();
            var mapped = Enumerable.Empty<TimePurchaseProjectRes>();

            _service.GetTimePurchaseProjectAsync("PRJ1").Returns(serviceResult);
            _mapper.Map<IEnumerable<TimePurchaseProjectRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTimePurchaseProject("PRJ1");

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetTimeSaleProfitCentre

        [Fact]
        public async Task GetTimeSaleProfitCentre_HappyPath_ReturnsOkWithMappedResult()
        {
            var serviceResult = new List<TimeSaleProfitCentreDto> { new() { ProfitCentre = "PC1" } };
            var mapped = new List<TimeSaleProfitCentreRes> { new() { ProfitCentre = "PC1" } };

            _service.GetTimeSaleProfitCentreAsync("PC1").Returns(serviceResult);
            _mapper.Map<IEnumerable<TimeSaleProfitCentreRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTimeSaleProfitCentre("PC1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTimeSaleProfitCentre_ServiceThrows_PropagatesException()
        {
            _service.GetTimeSaleProfitCentreAsync("PC1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetTimeSaleProfitCentre("PC1"));
        }

        [Fact]
        public async Task GetTimeSaleProfitCentre_EmptyResult_ReturnsOk()
        {
            var serviceResult = Enumerable.Empty<TimeSaleProfitCentreDto>();
            var mapped = Enumerable.Empty<TimeSaleProfitCentreRes>();

            _service.GetTimeSaleProfitCentreAsync("PC1").Returns(serviceResult);
            _mapper.Map<IEnumerable<TimeSaleProfitCentreRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTimeSaleProfitCentre("PC1");

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetTimeSaleWorkGroup

        [Fact]
        public async Task GetTimeSaleWorkGroup_HappyPath_ReturnsOkWithMappedResult()
        {
            var serviceResult = new List<TimeSaleWorkGroupDto> { new() { SellingWg = "WG1" } };
            var mapped = new List<TimeSaleWorkGroupRes> { new() { SellingWg = "WG1" } };
            var request = new TimeSaleWorkGroupReq { WorkGroup = "WG1" };

            _service.GetTimeSaleWorkGroupAsync("WG1").Returns(serviceResult);
            _mapper.Map<IEnumerable<TimeSaleWorkGroupRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTimeSaleWorkGroup(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTimeSaleWorkGroup_ServiceThrows_PropagatesException()
        {
            _service.GetTimeSaleWorkGroupAsync("WG1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetTimeSaleWorkGroup(new TimeSaleWorkGroupReq { WorkGroup = "WG1" }));
        }

        [Fact]
        public async Task GetTimeSaleWorkGroup_EmptyResult_ReturnsOk()
        {
            var serviceResult = Enumerable.Empty<TimeSaleWorkGroupDto>();
            var mapped = Enumerable.Empty<TimeSaleWorkGroupRes>();

            _service.GetTimeSaleWorkGroupAsync("WG1").Returns(serviceResult);
            _mapper.Map<IEnumerable<TimeSaleWorkGroupRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTimeSaleWorkGroup(new TimeSaleWorkGroupReq { WorkGroup = "WG1" });

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetTestSaleSellingWorkgroup

        [Fact]
        public async Task GetTestSaleSellingWorkgroup_HappyPath_ReturnsOkWithMappedResult()
        {
            var serviceResult = new List<TestSaleSellingWorkgroupDto> { new() { SellerWG = "WG1" } };
            var mapped = new List<TestSaleSellingWorkgroupRes> { new() { SellerWG = "WG1" } };

            _service.GetTestSaleSellingWorkgroupAsync("WG1").Returns(serviceResult);
            _mapper.Map<IEnumerable<TestSaleSellingWorkgroupRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTestSaleSellingWorkgroup("WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroup_ServiceThrows_PropagatesException()
        {
            _service.GetTestSaleSellingWorkgroupAsync("WG1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetTestSaleSellingWorkgroup("WG1"));
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroup_EmptyResult_ReturnsOk()
        {
            var serviceResult = Enumerable.Empty<TestSaleSellingWorkgroupDto>();
            var mapped = Enumerable.Empty<TestSaleSellingWorkgroupRes>();

            _service.GetTestSaleSellingWorkgroupAsync("WG1").Returns(serviceResult);
            _mapper.Map<IEnumerable<TestSaleSellingWorkgroupRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTestSaleSellingWorkgroup("WG1");

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetTestSaleBuyingProject

        [Fact]
        public async Task GetTestSaleBuyingProject_HappyPath_ReturnsOkWithMappedResult()
        {
            var serviceResult = new List<TestSaleBuyingProjectDto> { new() { Buyer = "PRJ1" } };
            var mapped = new List<TestSaleBuyingProjectRes> { new() { Buyer = "PRJ1" } };

            _service.GetTestSaleBuyingProjectAsync("PRJ1").Returns(serviceResult);
            _mapper.Map<IEnumerable<TestSaleBuyingProjectRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTestSaleBuyingProject("PRJ1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTestSaleBuyingProject_ServiceThrows_PropagatesException()
        {
            _service.GetTestSaleBuyingProjectAsync("PRJ1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetTestSaleBuyingProject("PRJ1"));
        }

        [Fact]
        public async Task GetTestSaleBuyingProject_EmptyResult_ReturnsOk()
        {
            var serviceResult = Enumerable.Empty<TestSaleBuyingProjectDto>();
            var mapped = Enumerable.Empty<TestSaleBuyingProjectRes>();

            _service.GetTestSaleBuyingProjectAsync("PRJ1").Returns(serviceResult);
            _mapper.Map<IEnumerable<TestSaleBuyingProjectRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTestSaleBuyingProject("PRJ1");

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion
    }
}
