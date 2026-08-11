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

namespace Apha.FPS.Api.UnitTests.Controller.MonthHourControllerTest
{
    public class MonthHourControllerTests
    {
        private readonly IMonthHourService _service;
        private readonly IMapper _mapper;
        private readonly MonthHourController _sut;

        public MonthHourControllerTests()
        {
            _service = Substitute.For<IMonthHourService>();
            _mapper = Substitute.For<IMapper>();
            _sut = new MonthHourController(_service, _mapper);
        }

        // -----------------------------------------------------------------------
        // GetAll
        // -----------------------------------------------------------------------

        #region GetAll

        [Fact]
        public async Task GetAll_WhenDataExists_ReturnsOkWithMappedPaginationRes()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<MonthHourDto>
            {
                Data = [
                    new MonthHourDto { Year = 2024, Month = 1, Days = 20, FpsYear = 2024 },
                    new MonthHourDto { Year = 2024, Month = 2, Days = 18, FpsYear = 2024 }
                ]
            };
            var mappedResult = new PaginationRes<MonthHourRes>
            {
                Data = [
                    new MonthHourRes { Year = 2024, Month = 1, Days = 20, FpsYear = 2024 },
                    new MonthHourRes { Year = 2024, Month = 2, Days = 18, FpsYear = 2024 }
                ]
            };

            _service.GetAllMonthHourAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<MonthHourRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetAll(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedResult);

            await _service.Received(1).GetAllMonthHourAsync(query);
            _mapper.Received(1).Map<PaginationRes<MonthHourRes>>(serviceResult);
        }

        [Fact]
        public async Task GetAll_WhenNoDataExists_ReturnsOkWithEmptyPaginationRes()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<MonthHourDto> { Data = [] };
            var mappedResult = new PaginationRes<MonthHourRes> { Data = [] };

            _service.GetAllMonthHourAsync(query).Returns(serviceResult);
            _mapper.Map<PaginationRes<MonthHourRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetAll(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);

            await _service.Received(1).GetAllMonthHourAsync(query);
        }

        [Fact]
        public async Task GetAll_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _service.GetAllMonthHourAsync(query).Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAll(query));
            exception.Message.Should().Be("Database error");
            await _service.Received(1).GetAllMonthHourAsync(query);
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetByYear
        // -----------------------------------------------------------------------

        #region GetByYear

        [Fact]
        public async Task GetByYear_WhenRecordsExist_ReturnsOkWithMappedList()
        {
            // Arrange
            const short year = 2024;
            var serviceResult = new List<MonthHourDto>
            {
                new MonthHourDto { Year = year, Month = 1, Days = 20, FpsYear = 2024 },
                new MonthHourDto { Year = year, Month = 2, Days = 18, FpsYear = 2024 }
            };
            var mappedResult = new List<MonthHourRes>
            {
                new MonthHourRes { Year = year, Month = 1, Days = 20, FpsYear = 2024 },
                new MonthHourRes { Year = year, Month = 2, Days = 18, FpsYear = 2024 }
            };

            _service.GetMonthHoursByYearAsync(year).Returns(serviceResult);
            _mapper.Map<IEnumerable<MonthHourRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetByYear(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedResult);

            await _service.Received(1).GetMonthHoursByYearAsync(year);
            _mapper.Received(1).Map<IEnumerable<MonthHourRes>>(serviceResult);
        }

        [Fact]
        public async Task GetByYear_WhenNoRecordsForYear_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            const short year = 2099;
            var empty = Enumerable.Empty<MonthHourDto>();
            _service.GetMonthHoursByYearAsync(year).Returns(empty);
            _mapper.Map<IEnumerable<MonthHourRes>>(empty).Returns(Enumerable.Empty<MonthHourRes>());

            // Act
            var result = await _sut.GetByYear(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            (okResult.Value as IEnumerable<MonthHourRes>).Should().BeEmpty();

            await _service.Received(1).GetMonthHoursByYearAsync(year);
        }

        [Fact]
        public async Task GetByYear_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const short year = 2024;
            _service.GetMonthHoursByYearAsync(year).Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetByYear(year));
            exception.Message.Should().Be("Database error");
            await _service.Received(1).GetMonthHoursByYearAsync(year);
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetDistinctYears
        // -----------------------------------------------------------------------

        #region GetDistinctYears

        [Fact]
        public async Task GetDistinctYears_WhenYearsExist_ReturnsOkWithYears()
        {
            // Arrange
            var years = new List<short> { 2022, 2023, 2024 };
            _service.GetDistinctYearsAsync().Returns(years.AsEnumerable());

            // Act
            var result = await _sut.GetDistinctYears();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(years);

            await _service.Received(1).GetDistinctYearsAsync();
        }

        [Fact]
        public async Task GetDistinctYears_WhenNoYearsExist_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            _service.GetDistinctYearsAsync().Returns(Enumerable.Empty<short>());

            // Act
            var result = await _sut.GetDistinctYears();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            (okResult.Value as IEnumerable<short>).Should().BeEmpty();

            await _service.Received(1).GetDistinctYearsAsync();
        }

        [Fact]
        public async Task GetDistinctYears_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetDistinctYearsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetDistinctYears());
            exception.Message.Should().Be("Database error");
            await _service.Received(1).GetDistinctYearsAsync();
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetYearEndMonthHours
        // -----------------------------------------------------------------------

        #region GetYearEndMonthHours

        [Fact]
        public async Task GetYearEndMonthHours_WhenDataExists_ReturnsOkWithMappedList()
        {
            // Arrange
            var serviceResult = new List<YearEndMonthHourDto>
            {
                new YearEndMonthHourDto { Month = 1, Days = 20, ExistsForPlannedYear = "Yes", FpsYear = 2025 },
                new YearEndMonthHourDto { Month = 2, Days = 18, ExistsForPlannedYear = "No",  FpsYear = 2025 }
            };
            var mappedResult = new List<YearEndMonthHourRes>
            {
                new YearEndMonthHourRes { Month = 1, Days = 20, ExistsForPlannedYear = "Yes", FpsYear = 2025 },
                new YearEndMonthHourRes { Month = 2, Days = 18, ExistsForPlannedYear = "No",  FpsYear = 2025 }
            };

            _service.GetYearEndMonthHoursAsync().Returns(serviceResult);
            _mapper.Map<List<YearEndMonthHourRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetYearEndMonthHours();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedResult);

            await _service.Received(1).GetYearEndMonthHoursAsync();
            _mapper.Received(1).Map<List<YearEndMonthHourRes>>(serviceResult);
        }

        [Fact]
        public async Task GetYearEndMonthHours_WhenNoDataExists_ReturnsOkWithEmptyList()
        {
            // Arrange
            var empty = new List<YearEndMonthHourDto>();
            _service.GetYearEndMonthHoursAsync().Returns(empty);
            _mapper.Map<List<YearEndMonthHourRes>>(empty).Returns(new List<YearEndMonthHourRes>());

            // Act
            var result = await _sut.GetYearEndMonthHours();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            (okResult.Value as List<YearEndMonthHourRes>).Should().BeEmpty();

            await _service.Received(1).GetYearEndMonthHoursAsync();
        }

        [Fact]
        public async Task GetYearEndMonthHours_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetYearEndMonthHoursAsync().Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetYearEndMonthHours());
            exception.Message.Should().Be("Database error");
            await _service.Received(1).GetYearEndMonthHoursAsync();
        }

        #endregion

        // -----------------------------------------------------------------------
        // Save
        // -----------------------------------------------------------------------

        #region Save

        [Fact]
        public async Task Save_WhenRequestIsValid_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new MonthHourReq { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };
            var dto = new MonthHourDto { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };
            var serviceResult = new MonthHourDto { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };
            var mappedRes = new MonthHourRes { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };

            _mapper.Map<MonthHourDto>(request).Returns(dto);
            _service.SaveMonthHourAsync(dto).Returns(serviceResult);
            _mapper.Map<MonthHourRes>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _sut.Save(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedRes);

            await _service.Received(1).SaveMonthHourAsync(dto);
            _mapper.Received(1).Map<MonthHourDto>(request);
            _mapper.Received(1).Map<MonthHourRes>(serviceResult);
        }

        [Fact]
        public async Task Save_WhenServiceThrowsBusinessValidationException_PropagatesException()
        {
            // Arrange
            var request = new MonthHourReq { Year = 2024, Month = 1, Days = -1 };
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = -1 };
            _mapper.Map<MonthHourDto>(request).Returns(dto);
            _service.SaveMonthHourAsync(dto)
                .Throws(new BusinessValidationErrorException([new BusinessValidationError("Invalid value", "Missing_Config")]));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.Save(request));
            await _service.Received(1).SaveMonthHourAsync(dto);
        }

        [Fact]
        public async Task Save_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new MonthHourReq { Year = 2024, Month = 1, Days = 20 };
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = 20 };
            _mapper.Map<MonthHourDto>(request).Returns(dto);
            _service.SaveMonthHourAsync(dto).Throws(new Exception("Save failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.Save(request));
            exception.Message.Should().Be("Save failed");
            await _service.Received(1).SaveMonthHourAsync(dto);
        }

        #endregion
    }
}
