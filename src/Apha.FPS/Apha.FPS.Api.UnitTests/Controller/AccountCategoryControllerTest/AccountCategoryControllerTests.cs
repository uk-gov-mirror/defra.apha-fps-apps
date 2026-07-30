using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPS.Api.UnitTests.Controller.AccountCategoryControllerTest
{
    public class AccountCategoryControllerTests
    {
        private const string TestAccShortName = "TEST001";
        private const string TestAccountDescription = "Test Description";
        private const string TestAccountType = "Income";

        private readonly IAccountCategoryService _service;
        private readonly IMapper _mapper;
        private readonly AccountCategoryController _controller;

        public AccountCategoryControllerTests()
        {
            _service = Substitute.For<IAccountCategoryService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AccountCategoryController(_service, _mapper);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ValidRequest_ReturnsOkWithPaginatedResult()
        {
            // Arrange
            var request = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<AccountCategoryDto>
            {
                Data = new List<AccountCategoryDto>
                {
                    CreateTestDto(TestAccShortName, TestAccountDescription)
                },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };
            var response = new PaginationRes<AccountCategoryRes>
            {
                Data = new List<AccountCategoryRes>
                {
                    CreateTestResponse(TestAccShortName, TestAccountDescription)
                },
                PaginationData = new Pagination { TotalRecords = 1 }
            };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParams);
            _service.GetAllAsync(queryParams, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<AccountCategoryRes>>(serviceResult).Returns(response);

            // Act
            var result = await _controller.GetAllAsync(request, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<AccountCategoryRes>>(okResult.Value);
            Assert.Single(data.Data);
        }

        [Fact]
        public async Task GetAllAsync_WithRcFilter_PassesFilterToService()
        {
            // Arrange
            var request = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<AccountCategoryDto> 
            { 
                Data = new List<AccountCategoryDto>(),
                PaginationData = new PaginationDto()
            };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParams);
            _service.GetAllAsync(queryParams, "rc").Returns(serviceResult);
            _mapper.Map<PaginationRes<AccountCategoryRes>>(serviceResult)
                .Returns(new PaginationRes<AccountCategoryRes> 
                { 
                    Data = new List<AccountCategoryRes>(),
                    PaginationData = new Pagination()
                });

            // Act
            await _controller.GetAllAsync(request, "rc");

            // Assert
            await _service.Received(1).GetAllAsync(queryParams, "rc");
        }

        [Fact]
        public async Task GetAllAsync_WithPsFilter_PassesFilterToService()
        {
            // Arrange
            var request = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<AccountCategoryDto> 
            { 
                Data = new List<AccountCategoryDto>(),
                PaginationData = new PaginationDto()
            };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParams);
            _service.GetAllAsync(queryParams, "ps").Returns(serviceResult);
            _mapper.Map<PaginationRes<AccountCategoryRes>>(serviceResult)
                .Returns(new PaginationRes<AccountCategoryRes> 
                { 
                    Data = new List<AccountCategoryRes>(),
                    PaginationData = new Pagination()
                });

            // Act
            await _controller.GetAllAsync(request, "ps");

            // Assert
            await _service.Received(1).GetAllAsync(queryParams, "ps");
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsOkWithEntity()
        {
            // Arrange
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);
            var response = CreateTestResponse(TestAccShortName, TestAccountDescription);

            _service.GetByIdAsync(TestAccShortName).Returns(dto);
            _mapper.Map<AccountCategoryRes>(dto).Returns(response);

            // Act
            var result = await _controller.GetByIdAsync(TestAccShortName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<AccountCategoryRes>(okResult.Value);
            Assert.Equal(TestAccShortName, data.AccShortName);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.GetByIdAsync("NONEXISTENT").Returns((AccountCategoryDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetByIdAsync("NONEXISTENT"));
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_ValidRequest_ReturnsOkWithCreatedEntity()
        {
            // Arrange
            var request = CreateTestRequest(TestAccShortName, TestAccountDescription);
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);
            var response = CreateTestResponse(TestAccShortName, TestAccountDescription);

            _mapper.Map<AccountCategoryDto>(request).Returns(dto);
            _service.AddAsync(dto).Returns(dto);
            _mapper.Map<AccountCategoryRes>(dto).Returns(response);

            // Act
            var result = await _controller.AddAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<AccountCategoryRes>(okResult.Value);
            Assert.Equal(TestAccShortName, data.AccShortName);
            await _service.Received(1).AddAsync(dto);
        }

        [Fact]
        public async Task AddAsync_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = CreateTestRequest(TestAccShortName, TestAccountDescription);
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);

            _mapper.Map<AccountCategoryDto>(request).Returns(dto);
            _service.AddAsync(dto).Returns<AccountCategoryDto>(x => throw new InvalidOperationException("Duplicate"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.AddAsync(request));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidRequest_ReturnsOkWithUpdatedEntity()
        {
            // Arrange
            var request = CreateTestRequest(TestAccShortName, "Updated Description");
            var dto = CreateTestDto(TestAccShortName, "Updated Description");
            var response = CreateTestResponse(TestAccShortName, "Updated Description");

            _mapper.Map<AccountCategoryDto>(request).Returns(dto);
            _service.UpdateAsync(TestAccShortName, dto).Returns(dto);
            _mapper.Map<AccountCategoryRes>(dto).Returns(response);

            // Act
            var result = await _controller.UpdateAsync(TestAccShortName, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<AccountCategoryRes>(okResult.Value);
            Assert.Equal("Updated Description", data.AccountDescription);
            await _service.Received(1).UpdateAsync(TestAccShortName, dto);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingEntity_PropagatesException()
        {
            // Arrange
            var request = CreateTestRequest(TestAccShortName, TestAccountDescription);
            var dto = CreateTestDto(TestAccShortName, TestAccountDescription);

            _mapper.Map<AccountCategoryDto>(request).Returns(dto);
            _service.UpdateAsync("NONEXISTENT", dto)
                .Returns<AccountCategoryDto>(x => throw new InvalidOperationException("Not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.UpdateAsync("NONEXISTENT", request));
        }

        [Fact]
        public async Task UpdateAsync_UsesOriginalAccShortNameParameter()
        {
            // Arrange
            var originalAccShortName = "ORIGINAL";
            var request = CreateTestRequest("CHANGED", TestAccountDescription);
            var dto = CreateTestDto("CHANGED", TestAccountDescription);

            _mapper.Map<AccountCategoryDto>(request).Returns(dto);
            _service.UpdateAsync(originalAccShortName, dto).Returns(dto);
            _mapper.Map<AccountCategoryRes>(dto).Returns(CreateTestResponse("CHANGED", TestAccountDescription));

            // Act
            await _controller.UpdateAsync(originalAccShortName, request);

            // Assert
            await _service.Received(1).UpdateAsync(originalAccShortName, dto);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingEntity_ReturnsOkWithTrue()
        {
            // Arrange
            _service.DeleteAsync(TestAccShortName).Returns(true);

            // Act
            var result = await _controller.DeleteAsync(TestAccShortName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _service.Received(1).DeleteAsync(TestAccShortName);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingEntity_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.DeleteAsync("NONEXISTENT").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.DeleteAsync("NONEXISTENT"));
        }

        #endregion

        #region Helper Methods

        private static AccountCategoryDto CreateTestDto(string accShortName, string accountDescription)
        {
            return new AccountCategoryDto
            {
                AccShortName = accShortName,
                AccountDescription = accountDescription,
                AccountType = TestAccountType,
                ConstituentAccountCodes = "1000,2000",
                ProjectSpecific = null,
                RcSpecific = null
            };
        }

        private static AccountCategoryReq CreateTestRequest(string accShortName, string accountDescription)
        {
            return new AccountCategoryReq
            {
                AccShortName = accShortName,
                AccountDescription = accountDescription,
                AccountType = TestAccountType,
                ConstituentAccountCodes = "1000,2000",
                ProjectSpecific = null,
                RcSpecific = null
            };
        }

        private static AccountCategoryRes CreateTestResponse(string accShortName, string accountDescription)
        {
            return new AccountCategoryRes
            {
                AccShortName = accShortName,
                AccountDescription = accountDescription,
                AccountType = TestAccountType,
                ConstituentAccountCodes = "1000,2000",
                ProjectSpecific = null,
                RcSpecific = null
            };
        }

        #endregion
    }
}
