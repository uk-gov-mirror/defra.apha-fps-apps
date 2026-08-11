using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.TestorProductControllerTest    
{
    /// <summary>
    /// Unit tests for TestListController (API Layer).
    /// Tests API validation, mapping, and exception handling.
    /// </summary>
    public class TestorProductControllerTests
    {
        private readonly ITestorProductService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly TestorProductController _controller;

        public TestorProductControllerTests()
        {
            _serviceMock = Substitute.For<ITestorProductService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new TestorProductController(_serviceMock, _mapperMock);
        }

        #region GetAllTestorProductsAsync

        [Fact]
        public async Task GetAllTestorProducts_ReturnsOkWithMappedList()
        {
            var dtos = new List<TestorProductDto>
            {
                new() { ItemCode = "T001", ItemDescription = "Test One", DefraUnitPrice = 50m, UnitPriceVla = 12.34m, FpsYear = 2025 },
                new() { ItemCode = "T002", ItemDescription = "Test Two", DefraUnitPrice = 100m, UnitPriceVla = 56.78m, FpsYear = 2025 }
            };
            _serviceMock.GetAllTestorProductsAsync().Returns(dtos);

            var result = await _controller.GetAllTestorProductsAsync();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<TestorProductRes>>(ok.Value, exactMatch: false);
            Assert.Equal(2, list.Count);
            Assert.Equal("T001", list[0].ItemCode);
            Assert.Equal(12.34m, list[0].UnitPriceVla);
            Assert.Equal(2025, list[0].FpsYear);
            Assert.Equal("T002", list[1].ItemCode);
            Assert.Equal(56.78m, list[1].UnitPriceVla);
        }

        [Fact]
        public async Task GetAllTestorProducts_EmptyList_ReturnsOkWithEmptyList()
        {
            _serviceMock.GetAllTestorProductsAsync().Returns(new List<TestorProductDto>());

            var result = await _controller.GetAllTestorProductsAsync();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<TestorProductRes>>(ok.Value, exactMatch: false);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetAllTestorProducts_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetAllTestorProductsAsync()
                .ThrowsAsync(new InvalidOperationException("DB error"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.GetAllTestorProductsAsync());
        }

        #endregion

        #region GetPaged

        [Fact]
        public async Task GetPaged_ValidQuery_ReturnsOkWithWrappedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<TestorProductDto> { new() { ItemCode = "TEST001", DefraUnitPrice = 100m } };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<TestorProductDto>(dtos, paginationData);
            var mappedResult = new PaginationRes<TestorProductRes>
            {
                Data = new List<TestorProductRes> { new() { ItemCode = "TEST001", DefraUnitPrice = 100m } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetPagedTestOrProductsAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestorProductRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<TestorProductRes>>(okResult.Value);
            Assert.Equal(mappedResult, response);
        }

        [Fact]
        public async Task GetPaged_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestorProductDto>(Enumerable.Empty<TestorProductDto>(), new PaginationDto());
            var mappedResult = new PaginationRes<TestorProductRes>();

            _serviceMock.GetPagedTestOrProductsAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestorProductRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PaginationRes<TestorProductRes>>(okResult.Value);
        }

        [Fact]
        public async Task GetPaged_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _serviceMock.GetPagedTestOrProductsAsync(query).ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPaged(query));
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ExistingItemCode_ReturnsOkWithMappedData()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var mapped = new TestorProductRes { ItemCode = "TEST001", DefraUnitPrice = 100m };

            _serviceMock.GetTestorProductByIdAsync("TEST001").Returns(dto);
            _mapperMock.Map<TestorProductRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetById("TEST001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetById_NonExistentItemCode_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.GetTestorProductByIdAsync("MISSING").Returns((TestorProductDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetById("MISSING"));
            Assert.Contains("MISSING", exception.Message);
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task GetById_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetTestorProductByIdAsync("TEST001").ThrowsAsync(new ArgumentException("Invalid itemCode"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetById("TEST001"));
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new TestorProductReq { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var createdDto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 };
            var mapped = new TestorProductRes { ItemCode = "TEST001", DefraUnitPrice = 100m };

            _mapperMock.Map<TestorProductDto>(request).Returns(dto);
            _serviceMock.CreateTestorProductAsync(dto).Returns(createdDto);
            _mapperMock.Map<TestorProductRes>(createdDto).Returns(mapped);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(TestorProductController.GetById), createdResult.ActionName);
            Assert.True(createdResult.RouteValues!.ContainsKey("itemCode"));
            Assert.Equal("TEST001", createdResult.RouteValues["itemCode"]);
            Assert.Equal(mapped, createdResult.Value);
        }

        [Fact]
        public async Task Create_ServiceThrowsArgumentException_PropagatesException()
        {
            // Arrange
            var request = new TestorProductReq { ItemCode = "TEST001", DefraUnitPrice = -1m };
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = -1m };

            _mapperMock.Map<TestorProductDto>(request).Returns(dto);
            _serviceMock.CreateTestorProductAsync(dto).ThrowsAsync(new ArgumentException("Validation failed"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Create(request));
        }

        [Fact]
        public async Task Create_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            // Arrange
            var request = new TestorProductReq { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m };

            _mapperMock.Map<TestorProductDto>(request).Returns(dto);
            _serviceMock.CreateTestorProductAsync(dto).ThrowsAsync(new InvalidOperationException("Failed to create"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(request));
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ValidRequest_ReturnsOkWithMappedData()
        {
            // Arrange
            var itemCode = "TEST001";
            var request = new TestorProductReq { DefraUnitPrice = 150m };
            var dto = new TestorProductDto { ItemCode = itemCode, DefraUnitPrice = 150m };
            var updatedDto = new TestorProductDto { ItemCode = itemCode, DefraUnitPrice = 150m, FpsYear = 2024 };
            var mapped = new TestorProductRes { ItemCode = itemCode, DefraUnitPrice = 150m };

            _mapperMock.Map<TestorProductDto>(request).Returns(dto);
            _serviceMock.UpdateTestorProductAsync(dto).Returns(updatedDto);
            _mapperMock.Map<TestorProductRes>(updatedDto).Returns(mapped);

            // Act
            var result = await _controller.Update(itemCode, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            Assert.Equal(itemCode, dto.ItemCode); // Verify itemCode was set
        }

        [Fact]
        public async Task Update_ItemCodeSetInDto_OverridesRequestValue()
        {
            // Arrange
            var itemCode = "TEST001";
            var request = new TestorProductReq { DefraUnitPrice = 150m };
            var dto = new TestorProductDto { DefraUnitPrice = 150m };
            var updatedDto = new TestorProductDto { ItemCode = itemCode, DefraUnitPrice = 150m };
            var mapped = new TestorProductRes { ItemCode = itemCode, DefraUnitPrice = 150m };

            _mapperMock.Map<TestorProductDto>(request).Returns(dto);
            _serviceMock.UpdateTestorProductAsync(Arg.Do<TestorProductDto>(d => Assert.Equal(itemCode, d.ItemCode))).Returns(updatedDto);
            _mapperMock.Map<TestorProductRes>(updatedDto).Returns(mapped);

            // Act
            var result = await _controller.Update(itemCode, request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Update_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            // Arrange
            var itemCode = "MISSING";
            var request = new TestorProductReq { DefraUnitPrice = 150m };
            var dto = new TestorProductDto { ItemCode = itemCode, DefraUnitPrice = 150m };

            _mapperMock.Map<TestorProductDto>(request).Returns(dto);
            _serviceMock.UpdateTestorProductAsync(dto).ThrowsAsync(new InvalidOperationException($"Test/Product with Item Code '{itemCode}' not found."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Update(itemCode, request));
            Assert.Contains(itemCode, exception.Message);
        }

        [Fact]
        public async Task Update_ServiceThrowsArgumentException_PropagatesException()
        {
            // Arrange
            var itemCode = "TEST001";
            var request = new TestorProductReq { DefraUnitPrice = -1m };
            var dto = new TestorProductDto { ItemCode = itemCode, DefraUnitPrice = -1m };

            _mapperMock.Map<TestorProductDto>(request).Returns(dto);
            _serviceMock.UpdateTestorProductAsync(dto).ThrowsAsync(new ArgumentException("Validation failed"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Update(itemCode, request));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_ExistingItemCode_ReturnsOkTrue()
        {
            // Arrange
            _serviceMock.DeleteTestorProductAsync("TEST001").Returns(true);

            // Act
            var result = await _controller.Delete("TEST001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task Delete_NonExistentItemCode_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteTestorProductAsync("MISSING").Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Delete("MISSING"));
            Assert.Contains("MISSING", exception.Message);
            Assert.Contains("not found for deletion", exception.Message);
        }

        [Fact]
        public async Task Delete_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteTestorProductAsync("TEST001").ThrowsAsync(new InvalidOperationException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Delete("TEST001"));
        }

        [Fact]
        public async Task Delete_ServiceThrowsArgumentException_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteTestorProductAsync("").ThrowsAsync(new ArgumentException("ItemCode cannot be empty"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Delete(""));
        }

        #endregion

        #region GetOwners

        [Fact]
        public async Task GetOwners_ReturnsOkWithOwnersList()
        {
            // Arrange
            var owners = new List<string> { "OW1", "OW2", "OW3" };
            _serviceMock.GetOwnersAsync().Returns(owners);

            // Act
            var result = await _controller.GetOwners();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(owners, okResult.Value);
        }

        [Fact]
        public async Task GetOwners_EmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            _serviceMock.GetOwnersAsync().Returns(new List<string>());

            // Act
            var result = await _controller.GetOwners();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultList = Assert.IsType<List<string>>(okResult.Value, exactMatch: false);
            Assert.Empty(resultList);
        }

        [Fact]
        public async Task GetOwners_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetOwnersAsync().ThrowsAsync(new InvalidOperationException("Failed to retrieve owners"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetOwners());
        }

        #endregion
    }
}
