using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.FpsSettingControllerTest
{
    public class FpsSettingControllerTest
    {
        private readonly IFpsSettingService _fpsSettingService;
        private readonly IMapper _mapper;
        private readonly FpsSettingController _sut;

        public FpsSettingControllerTest()
        {
            _fpsSettingService = Substitute.For<IFpsSettingService>();
            _mapper = Substitute.For<IMapper>();
            _sut = new FpsSettingController(_fpsSettingService, _mapper);
        }

        [Fact]
        public async Task GetAsync_WhenSettingsExist_ReturnsOkWithMappedSettings()
        {
            // Arrange
            var serviceResult = new List<FpsSettingDto>
            {
                new FpsSettingDto { Id = "1", Setting = "Setting1", Notes = "Value1" },
                new FpsSettingDto { Id = "2", Setting = "Setting2", Notes = "Value2" }
            };

            var expectedMappedResult = new List<FpsSettingRes>
            {
                new FpsSettingRes { },
                new FpsSettingRes { }
            };

            _fpsSettingService.GetAllSettingsAsync()
                .Returns(Task.FromResult(serviceResult));

            _mapper.Map<List<FpsSettingRes>>(serviceResult)
                .Returns(expectedMappedResult);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.StatusCode.Should().Be(200);
            okResult?.Value.Should().BeEquivalentTo(expectedMappedResult);

            await _fpsSettingService.Received(1).GetAllSettingsAsync();
            _mapper.Received(1).Map<List<FpsSettingRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAsync_WhenNoSettingsExist_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyServiceResult = new List<FpsSettingDto>();
            var emptyMappedResult = new List<FpsSettingRes>();

            _fpsSettingService.GetAllSettingsAsync()
                .Returns(Task.FromResult(emptyServiceResult));

            _mapper.Map<List<FpsSettingRes>>(emptyServiceResult)
                .Returns(emptyMappedResult);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.StatusCode.Should().Be(200);
            okResult?.Value.Should().BeEquivalentTo(emptyMappedResult);
            (okResult?.Value as List<FpsSettingRes>).Should().BeEmpty();

            await _fpsSettingService.Received(1).GetAllSettingsAsync();
            _mapper.Received(1).Map<List<FpsSettingRes>>(emptyServiceResult);
        }
               

        [Fact]
        public async Task GetAsync_WhenServiceThrowsException_ThrowsException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _fpsSettingService.GetAllSettingsAsync()
            .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAsync());

            exception.Message.Should().Be("Database connection failed");
            await _fpsSettingService.Received(1).GetAllSettingsAsync();
            _mapper.DidNotReceive().Map<List<FpsSettingRes>>(Arg.Any<object>());
        }

        #region GetHoursPerDayAsync

        [Fact]
        public async Task GetHoursPerDayAsync_WhenServiceReturnsValue_ReturnsOkWithDecimal()
        {
            // Arrange
            _fpsSettingService.GetHoursPerDayAsync().Returns(Task.FromResult(7.5m));

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(7.5m);
            await _fpsSettingService.Received(1).GetHoursPerDayAsync();
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenServiceReturnsDefaultValue_ReturnsOkWithEight()
        {
            // Arrange
            _fpsSettingService.GetHoursPerDayAsync().Returns(Task.FromResult(8m));

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(8m);
            await _fpsSettingService.Received(1).GetHoursPerDayAsync();
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _fpsSettingService.GetHoursPerDayAsync().Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetHoursPerDayAsync());
            exception.Message.Should().Be("Database connection failed");
            await _fpsSettingService.Received(1).GetHoursPerDayAsync();
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetYearEndSettingsAsync
        // -----------------------------------------------------------------------

        #region GetYearEndSettingsAsync

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenSettingsExist_ReturnsOkWithMappedList()
        {
            // Arrange
            var serviceResult = new List<YearEndFpsSettingDto>
            {
                new YearEndFpsSettingDto { Id = "HoursInDay", Setting = "8", ExistsForPlannedYear = "Yes" },
                new YearEndFpsSettingDto { Id = "CapApprovalReceivedForReset", Setting = "yes", ExistsForPlannedYear = "Yes" }
            };
            var mappedResult = new List<FpsYearEndSettingRes>
            {
                new FpsYearEndSettingRes { Id = "HoursInDay", Setting = "8", ExistsForPlannedYear = "Yes" },
                new FpsYearEndSettingRes { Id = "CapApprovalReceivedForReset", Setting = "yes", ExistsForPlannedYear = "Yes" }
            };

            _fpsSettingService.GetYearEndSettingsAsync().Returns(serviceResult);
            _mapper.Map<List<FpsYearEndSettingRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetYearEndSettingsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedResult);

            await _fpsSettingService.Received(1).GetYearEndSettingsAsync();
            _mapper.Received(1).Map<List<FpsYearEndSettingRes>>(serviceResult);
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenNoSettingsExist_ReturnsOkWithEmptyList()
        {
            // Arrange
            var serviceResult = new List<YearEndFpsSettingDto>();
            var mappedResult = new List<FpsYearEndSettingRes>();

            _fpsSettingService.GetYearEndSettingsAsync().Returns(serviceResult);
            _mapper.Map<List<FpsYearEndSettingRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetYearEndSettingsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            (okResult.Value as List<FpsYearEndSettingRes>).Should().BeEmpty();

            await _fpsSettingService.Received(1).GetYearEndSettingsAsync();
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _fpsSettingService.GetYearEndSettingsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetYearEndSettingsAsync());
            exception.Message.Should().Be("Database error");
            await _fpsSettingService.Received(1).GetYearEndSettingsAsync();
        }

        #endregion

        // -----------------------------------------------------------------------
        // PostAsync
        // -----------------------------------------------------------------------

        #region PostAsync

        [Fact]
        public async Task PostAsync_WhenRequestIsValid_ReturnsCreatedAtActionWithMappedResult()
        {
            // Arrange
            var request = new FpsSettingReq { Id = "NewKey", Setting = "10" };
            var dto = new FpsSettingDto { Id = "NewKey", Setting = "10" };
            var serviceResult = new FpsSettingDto { Id = "NewKey", Setting = "10" };
            var mappedRes = new FpsSettingRes { Id = "NewKey", Setting = "10" };

            _mapper.Map<FpsSettingDto>(request).Returns(dto);
            _fpsSettingService.AddSettingAsync(dto).Returns(serviceResult);
            _mapper.Map<FpsSettingRes>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _sut.PostAsync(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().BeEquivalentTo(mappedRes);

            await _fpsSettingService.Received(1).AddSettingAsync(dto);
            _mapper.Received(1).Map<FpsSettingDto>(request);
            _mapper.Received(1).Map<FpsSettingRes>(serviceResult);
        }

        [Fact]
        public async Task PostAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new FpsSettingReq { Id = "NewKey", Setting = "10" };
            var dto = new FpsSettingDto { Id = "NewKey", Setting = "10" };
            _mapper.Map<FpsSettingDto>(request).Returns(dto);
            _fpsSettingService.AddSettingAsync(dto).Throws(new Exception("Insert failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.PostAsync(request));
            exception.Message.Should().Be("Insert failed");
            await _fpsSettingService.Received(1).AddSettingAsync(dto);
        }

        #endregion

        // -----------------------------------------------------------------------
        // PutAsync
        // -----------------------------------------------------------------------

        #region PutAsync

        [Fact]
        public async Task PutAsync_WhenRequestIsValid_ReturnsOkWithMappedResult()
        {
            // Arrange
            const string id = "HoursInDay";
            var request = new FpsSettingReq { Setting = "9" };
            var dto = new FpsSettingDto { Id = id, Setting = "9" };
            var serviceResult = new FpsSettingDto { Id = id, Setting = "9" };
            var mappedRes = new FpsSettingRes { Id = id, Setting = "9" };

            _mapper.Map<FpsSettingDto>(Arg.Is<FpsSettingReq>(r => r.Id == id)).Returns(dto);
            _fpsSettingService.UpdateSettingAsync(dto).Returns(serviceResult);
            _mapper.Map<FpsSettingRes>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _sut.PutAsync(id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedRes);

            await _fpsSettingService.Received(1).UpdateSettingAsync(dto);
        }

        [Fact]
        public async Task PutAsync_SetsRequestIdFromRouteId()
        {
            // Arrange
            const string routeId = "HoursInDay";
            var request = new FpsSettingReq { Id = "OriginalId", Setting = "9" };
            var dto = new FpsSettingDto { Id = routeId, Setting = "9" };
            var serviceResult = new FpsSettingDto { Id = routeId };
            var mappedRes = new FpsSettingRes { Id = routeId };

            _mapper.Map<FpsSettingDto>(Arg.Is<FpsSettingReq>(r => r.Id == routeId)).Returns(dto);
            _fpsSettingService.UpdateSettingAsync(dto).Returns(serviceResult);
            _mapper.Map<FpsSettingRes>(serviceResult).Returns(mappedRes);

            // Act
            await _sut.PutAsync(routeId, request);

            // Assert — the route id must override the body id before mapping
            request.Id.Should().Be(routeId);
        }

        [Fact]
        public async Task PutAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string id = "HoursInDay";
            var request = new FpsSettingReq { Id = id, Setting = "9" };
            var dto = new FpsSettingDto { Id = id, Setting = "9" };
            _mapper.Map<FpsSettingDto>(Arg.Any<FpsSettingReq>()).Returns(dto);
            _fpsSettingService.UpdateSettingAsync(dto).Throws(new Exception("Update failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.PutAsync(id, request));
            exception.Message.Should().Be("Update failed");
            await _fpsSettingService.Received(1).UpdateSettingAsync(dto);
        }

        #endregion

        // -----------------------------------------------------------------------
        // SaveAsync
        // -----------------------------------------------------------------------

        #region SaveAsync

        [Fact]
        public async Task SaveAsync_WhenRequestIsValid_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new FpsSettingReq { Id = "HoursInDay", Setting = "8" };
            var dto = new FpsSettingDto { Id = "HoursInDay", Setting = "8" };
            var serviceResult = new FpsSettingDto { Id = "HoursInDay", Setting = "8" };
            var mappedRes = new FpsSettingRes { Id = "HoursInDay", Setting = "8" };

            _mapper.Map<FpsSettingDto>(request).Returns(dto);
            _fpsSettingService.SaveSettingAsync(dto).Returns(serviceResult);
            _mapper.Map<FpsSettingRes>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _sut.SaveAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedRes);

            await _fpsSettingService.Received(1).SaveSettingAsync(dto);
            _mapper.Received(1).Map<FpsSettingDto>(request);
            _mapper.Received(1).Map<FpsSettingRes>(serviceResult);
        }

        [Fact]
        public async Task SaveAsync_WhenServiceThrowsBusinessValidationException_PropagatesException()
        {
            // Arrange
            var request = new FpsSettingReq { Id = "HoursInDay", Setting = "invalid" };
            var dto = new FpsSettingDto { Id = "HoursInDay", Setting = "invalid" };
            _mapper.Map<FpsSettingDto>(request).Returns(dto);
            _fpsSettingService.SaveSettingAsync(dto)
                .Throws(new BusinessValidationErrorException([new BusinessValidationError("Invalid value", "Missing_HoursInDay")]));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.SaveAsync(request));
            await _fpsSettingService.Received(1).SaveSettingAsync(dto);
        }

        [Fact]
        public async Task SaveAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new FpsSettingReq { Id = "OtherKey", Setting = "value" };
            var dto = new FpsSettingDto { Id = "OtherKey", Setting = "value" };
            _mapper.Map<FpsSettingDto>(request).Returns(dto);
            _fpsSettingService.SaveSettingAsync(dto).Throws(new Exception("Save failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.SaveAsync(request));
            exception.Message.Should().Be("Save failed");
            await _fpsSettingService.Received(1).SaveSettingAsync(dto);
        }

        #endregion
    }
}
