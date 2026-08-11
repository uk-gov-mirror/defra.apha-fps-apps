using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.AccessSystemControllerTest
{
    public class AccessSystemControllerTests
    {
        private readonly IAccessSystemService _service;
        private readonly IMapper _mapper;
        private readonly AccessSystemController _controller;

        public AccessSystemControllerTests()
        {
            _service    = Substitute.For<IAccessSystemService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new AccessSystemController(_service, _mapper);
        }

        private static AccessSystemDto MakeDto(int systemid = 1, string name = "PIMS") =>
            new() { SystemId = systemid, SystemName = name };

        private static AccessSystemRes MakeRes(int systemid = 1, string name = "PIMS") =>
            new() { SystemId = systemid, SystemName = name };

        #region GetAll

        [Fact]
        public async Task GetAll_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos    = new List<AccessSystemDto> { MakeDto(1, "PIMS"), MakeDto(2, "PACT") };
            var resList = new List<AccessSystemRes> { MakeRes(1, "PIMS"), MakeRes(2, "PACT") };
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<AccessSystemRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<AccessSystemRes>>(ok.Value);
            Assert.Equal(2, returned.Count);
            await _service.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAll_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos    = new List<AccessSystemDto>();
            var resList = new List<AccessSystemRes>();
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<AccessSystemRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<AccessSystemRes>>(ok.Value));
        }

        [Fact]
        public async Task GetAll_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetAllAsync().ThrowsAsync(new Exception("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll());
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dto = MakeDto(1, "PIMS");
            var res = MakeRes(1, "PIMS");
            _service.GetByIdAsync(1).Returns(dto);
            _mapper.Map<AccessSystemRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetByIdAsync(1);
        }

        [Fact]
        public async Task GetById_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            _service.GetByIdAsync(Arg.Any<int>()).Returns((AccessSystemDto?)null);

            // Act
            var result = await _controller.GetById(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion
    }
}
