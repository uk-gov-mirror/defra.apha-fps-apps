using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.AnimalRequestControllerTest
{
    public class AnimalRequestControllerTests
    {
        private readonly IAnimalService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly AnimalRequestController _controller;

        public AnimalRequestControllerTests()
        {
            _serviceMock = Substitute.For<IAnimalService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new AnimalRequestController(_serviceMock, _mapperMock);
        }

        #region GetAnimalCostAsync

        [Fact]
        public async Task GetAnimalCostAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string>();
            var mappedQuery = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<AnimalCostViewDto>();
            var mappedResult = new PaginationRes<AnimalCostViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetAnimalCostAsync(mappedQuery, "JOB001").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<AnimalCostViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAnimalCostAsync(query, "JOB001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAnimalCostAsync_EdgeCase_EmptyResult_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string>();
            var mappedQuery = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<AnimalCostViewDto>();
            var mappedResult = new PaginationRes<AnimalCostViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<AnimalCostViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAnimalCostAsync(query, "");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAnimalCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalCostAsync(query, "JOB001"));
        }

        [Fact]
        public async Task GetAnimalCostAsync_MapperThrows_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalCostAsync(query, "JOB001"));
        }

        #endregion

        #region GetAnimalCostByAnimalTypeAsync

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string>();
            var mappedQuery = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<AnimalCostViewDto>();
            var mappedResult = new PaginationRes<AnimalCostViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetAnimalCostByAnimalTypeAsync(mappedQuery, "CATTLE").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<AnimalCostViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_EdgeCase_EmptyResult_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string>();
            var mappedQuery = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<AnimalCostViewDto>();
            var mappedResult = new PaginationRes<AnimalCostViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetAnimalCostByAnimalTypeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<AnimalCostViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAnimalCostByAnimalTypeAsync(query, "");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetAnimalCostByAnimalTypeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalCostByAnimalTypeAsync(query, "CATTLE"));
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_MapperThrows_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalCostByAnimalTypeAsync(query, "CATTLE"));
        }

        #endregion

        #region GetAnimalLookupAsync

        [Fact]
        public async Task GetAnimalLookupAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<AnimalDto>
            {
                new AnimalDto { AnimalType = "CAT", DailyRate = 50.00m }
            };
            var mappedResult = new List<AnimalRes>
            {
                new AnimalRes { AnimalType = "CAT", DailyRate = 50.00m }
            };

            _serviceMock.GetAnimalLookupAsync().Returns(serviceResult);
            _mapperMock.Map<List<AnimalRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAnimalLookupAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAnimalLookupAsync_EdgeCase_EmptyList_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<AnimalDto>();
            var mappedResult = new List<AnimalRes>();

            _serviceMock.GetAnimalLookupAsync().Returns(serviceResult);
            _mapperMock.Map<List<AnimalRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAnimalLookupAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Empty((List<AnimalRes>)okResult.Value!);
        }

        [Fact]
        public async Task GetAnimalLookupAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAnimalLookupAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalLookupAsync());
        }

        [Fact]
        public async Task GetAnimalLookupAsync_MapperThrows_PropagatesException()
        {
            // Arrange
            var serviceResult = new List<AnimalDto>();
            _serviceMock.GetAnimalLookupAsync().Returns(serviceResult);
            _mapperMock.Map<List<AnimalRes>>(serviceResult).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalLookupAsync());
        }

        #endregion

        #region GetAnimalRateByIdAsync

        [Fact]
        public async Task GetAnimalRateByIdAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.GetAnimalRateByIdAsync("CAT", "JOB001").Returns(Task.FromResult<decimal?>(75.50m));

            // Act
            var result = await _controller.GetAnimalRateByIdAsync("CAT", "JOB001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(75.50m, okResult.Value);
        }

        [Fact]
        public async Task GetAnimalRateByIdAsync_NotFound_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.GetAnimalRateByIdAsync("UNKNOWN", "JOB001").Returns(Task.FromResult<decimal?>(null));

            // Act
            var result = await _controller.GetAnimalRateByIdAsync("UNKNOWN", "JOB001");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAnimalRateByIdAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAnimalRateByIdAsync("CAT", "JOB001").Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalRateByIdAsync("CAT", "JOB001"));
        }

        #endregion

        #region AddAnimalCostAsync

        [Fact]
        public async Task AddAnimalCostAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req = new AnimalRequestReq { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 };
            var dto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 };
            var resultDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 };
            var mapped = new AnimalRequestRes { JobCode = "JOB001", AnimalType = "CAT" };

            _mapperMock.Map<AnimalRequestDto>(req).Returns(dto);
            _serviceMock.AddAnimalCostAsync(dto).Returns(resultDto);
            _mapperMock.Map<AnimalRequestRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.AddAnimalCostAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task AddAnimalCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new AnimalRequestReq { JobCode = "JOB001", AnimalType = "CAT" };
            var dto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT" };

            _mapperMock.Map<AnimalRequestDto>(req).Returns(dto);
            _serviceMock.AddAnimalCostAsync(dto).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.AddAnimalCostAsync(req));
        }

        [Fact]
        public async Task AddAnimalCostAsync_MapperThrows_PropagatesException()
        {
            // Arrange
            var req = new AnimalRequestReq { JobCode = "JOB001", AnimalType = "CAT" };
            _mapperMock.Map<AnimalRequestDto>(req).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.AddAnimalCostAsync(req));
        }

        #endregion

        #region UpdateAnimalCostAsync

        [Fact]
        public async Task UpdateAnimalCostAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req = new AnimalRequestReq { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 7, NumberOfAnimals = 12 };
            var dto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 7, NumberOfAnimals = 12 };
            var resultDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 7, NumberOfAnimals = 12 };
            var mapped = new AnimalRequestRes { JobCode = "JOB001", AnimalType = "CAT" };

            _mapperMock.Map<AnimalRequestDto>(req).Returns(dto);
            _serviceMock.UpdateAnimalCostAsync(dto).Returns(resultDto);
            _mapperMock.Map<AnimalRequestRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.UpdateAnimalCostAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new AnimalRequestReq { JobCode = "JOB999", AnimalType = "CAT" };
            var dto = new AnimalRequestDto { JobCode = "JOB999", AnimalType = "CAT" };

            _mapperMock.Map<AnimalRequestDto>(req).Returns(dto);
            _serviceMock.UpdateAnimalCostAsync(dto).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateAnimalCostAsync(req));
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_MapperThrows_PropagatesException()
        {
            // Arrange
            var req = new AnimalRequestReq { JobCode = "JOB001", AnimalType = "CAT" };
            _mapperMock.Map<AnimalRequestDto>(req).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateAnimalCostAsync(req));
        }

        #endregion

        #region DeleteAnimalCostAsync

        [Fact]
        public async Task DeleteAnimalCostAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteAnimalCostAsync(1).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.DeleteAnimalCostAsync(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteAnimalCostAsync_NotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteAnimalCostAsync(999).Returns(Task.FromResult(false));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _controller.DeleteAnimalCostAsync(999)
            );
        }

        [Fact]
        public async Task DeleteAnimalCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteAnimalCostAsync(1).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteAnimalCostAsync(1));
        }

        #endregion

        #region GetTotalAnimalCostAsync

        [Fact]
        public async Task GetTotalAnimalCostAsync_HappyPath_ReturnsOkWithTotal()
        {
            // Arrange
            _serviceMock.GetTotalAnimalCostAsync("JOB001").Returns(Task.FromResult(250.00m));

            // Act
            var result = await _controller.GetTotalAnimalCostAsync("JOB001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(250.00m, okResult.Value);
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_EdgeCase_ReturnsOkWithZero()
        {
            // Arrange
            _serviceMock.GetTotalAnimalCostAsync("EMPTY").Returns(Task.FromResult(0m));

            // Act
            var result = await _controller.GetTotalAnimalCostAsync("EMPTY");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0m, okResult.Value);
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetTotalAnimalCostAsync("JOB001")
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetTotalAnimalCostAsync("JOB001"));
        }

        #endregion

        #region GetAnimalCostViewByIdAsync

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new AnimalCostViewDto
            {
                IndCounter     = 1,
                JobCode        = "JOB001",
                AnimalType     = "CAT",
                NumberOfDays   = 5,
                NumberOfAnimals = 2,
                AnimalCost     = 100m
            };
            var mappedResult = new AnimalCostViewRes
            {
                IndCounter     = 1,
                JobCode        = "JOB001",
                AnimalType     = "CAT",
                NumberOfDays   = 5,
                NumberOfAnimals = 2,
                AnimalCost     = 100m
            };

            _serviceMock.GetAnimalCostViewByIdAsync(1, "JOB001").Returns(serviceResult);
            _mapperMock.Map<AnimalCostViewRes>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAnimalCostViewByIdAsync(1, "JOB001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_NullResult_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.GetAnimalCostViewByIdAsync(999, "JOB001")
                .Returns(Task.FromResult<AnimalCostViewDto?>(null));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _controller.GetAnimalCostViewByIdAsync(999, "JOB001"));
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAnimalCostViewByIdAsync(1, "JOB001")
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetAnimalCostViewByIdAsync(1, "JOB001"));
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_MapperThrows_PropagatesException()
        {
            // Arrange
            var serviceResult = new AnimalCostViewDto { IndCounter = 1, JobCode = "JOB001" };

            _serviceMock.GetAnimalCostViewByIdAsync(1, "JOB001").Returns(serviceResult);
            _mapperMock.Map<AnimalCostViewRes>(serviceResult)
                .Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetAnimalCostViewByIdAsync(1, "JOB001"));
        }

        #endregion

        #region GetAnimalSnapshotAsync

        [Fact]
        public async Task GetAnimalSnapshotAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string>();
            var mappedQuery = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<AnimalSnapshotViewDto>();
            var mappedResult = new PaginationRes<AnimalSnapshotViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetAnimalSnapshotAsync(mappedQuery).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<AnimalSnapshotViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAnimalSnapshotAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetAnimalSnapshotAsync(mappedQuery);
        }

        [Fact]
        public async Task GetAnimalSnapshotAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new PaginationReq<string>();
            var mappedQuery = new QueryParameters<string>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetAnimalSnapshotAsync(mappedQuery)
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalSnapshotAsync(query));
        }

        #endregion
    }
}