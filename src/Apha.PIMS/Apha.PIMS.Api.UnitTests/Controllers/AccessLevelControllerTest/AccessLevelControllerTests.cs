using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.AccessLevelControllerTest
{
    public class AccessLevelControllerTests
    {
        private readonly IAccessLevelService _service;
        private readonly IMapper _mapper;
        private readonly AccessLevelController _controller;

        public AccessLevelControllerTests()
        {
            _service    = Substitute.For<IAccessLevelService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new AccessLevelController(_service, _mapper);
        }

        private static AccessLevelDto MakeDto(int systemid = 1, int accesslevelid = 10, string name = "Level 1") =>
            new() { SystemId = systemid, AccessLevelId = accesslevelid, AccessLevelName = name };

        private static AccessLevelRes MakeRes(int systemid = 1, int accesslevelid = 10, string name = "Level 1") =>
            new() { SystemId = systemid, AccessLevelId = accesslevelid, AccessLevelName = name };

        #region GetAll

        [Fact]
        public async Task GetAll_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos    = new List<AccessLevelDto> { MakeDto(1, 1, "Read"), MakeDto(1, 2, "Write") };
            var resList = new List<AccessLevelRes> { MakeRes(1, 1, "Read"), MakeRes(1, 2, "Write") };
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<AccessLevelRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<AccessLevelRes>>(ok.Value);
            Assert.Equal(2, returned.Count);
            await _service.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAll_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos    = new List<AccessLevelDto>();
            var resList = new List<AccessLevelRes>();
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<AccessLevelRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<AccessLevelRes>>(ok.Value));
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

        #region GetBySystemId

        [Fact]
        public async Task GetBySystemId_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos    = new List<AccessLevelDto> { MakeDto(2, 3, "Admin") };
            var resList = new List<AccessLevelRes> { MakeRes(2, 3, "Admin") };
            _service.GetBySystemIdAsync(2).Returns(dtos);
            _mapper.Map<List<AccessLevelRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetBySystemId(2);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Single(Assert.IsType<List<AccessLevelRes>>(ok.Value));
            await _service.Received(1).GetBySystemIdAsync(2);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dto = MakeDto(1, 10, "Read");
            var res = MakeRes(1, 10, "Read");
            _service.GetByIdAsync(1, 10).Returns(dto);
            _mapper.Map<AccessLevelRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetById(1, 10);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetByIdAsync(1, 10);
        }

        [Fact]
        public async Task GetById_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            _service.GetByIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns((AccessLevelDto?)null);

            // Act
            var result = await _controller.GetById(99, 88);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtActionWithMappedResult()
        {
            // Arrange
            var req     = MakeRes(1, 0, "Editor");
            var dto     = MakeDto(1, 0, "Editor");
            var created = MakeDto(1, 7, "Editor");
            var res     = MakeRes(1, 7, "Editor");
            _mapper.Map<AccessLevelDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns(created);
            _mapper.Map<AccessLevelRes>(created).Returns(res);

            // Act
            var result = await _controller.Create(req);

            // Assert
            var created201 = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(AccessLevelController.GetById), created201.ActionName);
            Assert.Equal(res, created201.Value);
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dto     = MakeDto(1, 7, "Editor");
            var updated = MakeDto(1, 7, "Editor+");
            var res     = MakeRes(1, 7, "Editor+");
            _mapper.Map<AccessLevelDto>(Arg.Any<AccessLevelRes>()).Returns(dto);
            _service.UpdateAsync(Arg.Any<AccessLevelDto>()).Returns(updated);
            _mapper.Map<AccessLevelRes>(updated).Returns(res);

            // Act
            var result = await _controller.Update(1, 7, MakeRes());

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task Update_SetsCompositePkOnDtoBeforeCallingService()
        {
            // Arrange
            var dto = new AccessLevelDto { SystemId = 0, AccessLevelId = 0, AccessLevelName = "from mapper" };
            _mapper.Map<AccessLevelDto>(Arg.Any<AccessLevelRes>()).Returns(dto);
            _service.UpdateAsync(Arg.Any<AccessLevelDto>()).Returns(MakeDto(5, 22, "Updated"));
            _mapper.Map<AccessLevelRes>(Arg.Any<AccessLevelDto>()).Returns(MakeRes(5, 22, "Updated"));

            // Act
            await _controller.Update(5, 22, MakeRes());

            // Assert
            await _service.Received(1).UpdateAsync(
                Arg.Is<AccessLevelDto>(d => d.SystemId == 5 && d.AccessLevelId == 22));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_ServiceCompletes_ReturnsOkWithSuccessTrue()
        {
            // Act
            var result = await _controller.Delete(1, 7);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True(Assert.IsType<bool>(ok.Value));
            await _service.Received(1).DeleteAsync(1, 7);
        }

        [Fact]
        public async Task Delete_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _service.DeleteAsync(Arg.Any<int>(), Arg.Any<int>())
                    .ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Delete(99, 88));
        }

        #endregion
    }
}
