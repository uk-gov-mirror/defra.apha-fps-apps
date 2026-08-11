using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.AccessUserLevelControllerTest
{
    public class AccessUserLevelControllerTests
    {
        private readonly IAccessUserLevelService _service;
        private readonly IMapper _mapper;
        private readonly AccessUserLevelController _controller;

        public AccessUserLevelControllerTests()
        {
            _service    = Substitute.For<IAccessUserLevelService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new AccessUserLevelController(_service, _mapper);
        }

        private static AccessUserLevelDto MakeDto(int systemid = 1, string ntlogin = "DOMAIN\\user1", int accesslevelid = 10) =>
            new() { SystemId = systemid, NtLogin = ntlogin, AccessLevelId = accesslevelid };

        private static AccessUserLevelRes MakeRes(int systemid = 1, string ntlogin = "DOMAIN\\user1", int accesslevelid = 10) =>
            new() { SystemId = systemid, NtLogin = ntlogin, AccessLevelId = accesslevelid };

        private static AccessUserLevelReq MakeReq(int systemid = 1, string ntlogin = "DOMAIN\\user1", int accesslevelid = 10) =>
            new() { SystemId = systemid, NtLogin = ntlogin, AccessLevelId = accesslevelid };

        #region GetPagedAccessUserLevelAll

        [Fact]
        public async Task GetPagedAccessUserLevelAll_ServiceReturnsData_ReturnsOkWithMappedPaginationResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "dom" };
            var dtos = new List<AccessUserLevelDto> { MakeDto(1, "dom\\u1", 1), MakeDto(1, "dom\\u2", 2) };
            var pagedDto = new PaginatedResult<AccessUserLevelDto>(dtos, new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 });

            var resList = new List<AccessUserLevelRes> { MakeRes(1, "dom\\u1", 1), MakeRes(1, "dom\\u2", 2) };
            var pageRes = new PaginationRes<AccessUserLevelRes>(resList, new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 });

            _service.GetPagedAccessUserLevelAllAsync(query).Returns(pagedDto);
            _mapper.Map<PaginationRes<AccessUserLevelRes>>(pagedDto).Returns(pageRes);

            // Act
            var result = await _controller.GetPagedAccessUserLevelAll(query);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pageRes, ok.Value);
            await _service.Received(1).GetPagedAccessUserLevelAllAsync(query);
            _mapper.Received(1).Map<PaginationRes<AccessUserLevelRes>>(pagedDto);
        }

        [Fact]
        public async Task GetPagedAccessUserLevelAll_ServiceReturnsEmptyData_ReturnsOkWithEmptyPaginationResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedDto = new PaginatedResult<AccessUserLevelDto>(new List<AccessUserLevelDto>(), new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var pageRes = new PaginationRes<AccessUserLevelRes>(new List<AccessUserLevelRes>(), new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _service.GetPagedAccessUserLevelAllAsync(query).Returns(pagedDto);
            _mapper.Map<PaginationRes<AccessUserLevelRes>>(pagedDto).Returns(pageRes);

            // Act
            var result = await _controller.GetPagedAccessUserLevelAll(query);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pageRes, ok.Value);
        }

        [Fact]
        public async Task GetPagedAccessUserLevelAll_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPagedAccessUserLevelAllAsync(query).ThrowsAsync(new Exception("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedAccessUserLevelAll(query));
        }

        #endregion

        #region GetBySystemId

        [Fact]
        public async Task GetBySystemId_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos    = new List<AccessUserLevelDto> { MakeDto(2, "dom\\u1", 1) };
            var resList = new List<AccessUserLevelRes> { MakeRes(2, "dom\\u1", 1) };
            _service.GetBySystemIdAsync(2).Returns(dtos);
            _mapper.Map<List<AccessUserLevelRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetBySystemId(2);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Single(Assert.IsType<List<AccessUserLevelRes>>(ok.Value));
            await _service.Received(1).GetBySystemIdAsync(2);
        }

        [Fact]
        public async Task GetBySystemId_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            _service.GetBySystemIdAsync(99).Returns(new List<AccessUserLevelDto>());
            _mapper.Map<List<AccessUserLevelRes>>(Arg.Any<List<AccessUserLevelDto>>()).Returns(new List<AccessUserLevelRes>());

            // Act
            var result = await _controller.GetBySystemId(99);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<AccessUserLevelRes>>(ok.Value));
        }

        #endregion

        #region GetByUser

        [Fact]
        public async Task GetByUser_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            const string encodedLogin = "DOMAIN%5Cuser1";
            const string decodedLogin = "DOMAIN\\user1";
            var dtos    = new List<AccessUserLevelDto> { MakeDto(1, decodedLogin, 2) };
            var resList = new List<AccessUserLevelRes> { MakeRes(1, decodedLogin, 2) };
            _service.GetByUserAsync(1, decodedLogin).Returns(dtos);
            _mapper.Map<List<AccessUserLevelRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetByUser(1, encodedLogin);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Single(Assert.IsType<List<AccessUserLevelRes>>(ok.Value));
            await _service.Received(1).GetByUserAsync(1, decodedLogin);
        }

        [Fact]
        public async Task GetByUser_DecodesNtLoginBeforeServiceCall()
        {
            // Arrange
            const string encodedLogin = "dom%5Cjsmith";
            const string decodedLogin = "dom\\jsmith";
            _service.GetByUserAsync(1, decodedLogin).Returns(new List<AccessUserLevelDto> { MakeDto(1, decodedLogin, 1) });
            _mapper.Map<List<AccessUserLevelRes>>(Arg.Any<List<AccessUserLevelDto>>()).Returns(new List<AccessUserLevelRes> { MakeRes(1, decodedLogin, 1) });

            // Act
            await _controller.GetByUser(1, encodedLogin);

            // Assert
            await _service.Received(1).GetByUserAsync(1, decodedLogin);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            const string encodedLogin = "DOMAIN%5Cuser1";
            const string decodedLogin = "DOMAIN\\user1";
            var dto = MakeDto(1, decodedLogin, 2);
            var res = MakeRes(1, decodedLogin, 2);
            _service.GetByIdAsync(1, decodedLogin, 2).Returns(dto);
            _mapper.Map<AccessUserLevelRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetById(1, encodedLogin, 2);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetByIdAsync(1, decodedLogin, 2);
        }

        [Fact]
        public async Task GetById_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            _service.GetByIdAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns((AccessUserLevelDto?)null);

            // Act
            var result = await _controller.GetById(99, "unknown", 88);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtActionWithMappedResult()
        {
            // Arrange
            var req     = MakeReq(1, "dom\\user", 2);
            var dto     = MakeDto(1, "dom\\user", 2);
            var created = MakeDto(1, "dom\\user", 2);
            var res     = MakeRes(1, "dom\\user", 2);
            _mapper.Map<AccessUserLevelDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns(created);
            _mapper.Map<AccessUserLevelRes>(created).Returns(res);

            // Act
            var result = await _controller.Create(req);

            // Assert
            var created201 = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(AccessUserLevelController.GetById), created201.ActionName);
            Assert.Equal(res, created201.Value);
        }

        [Fact]
        public async Task Create_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            // Arrange
            _mapper.Map<AccessUserLevelDto>(Arg.Any<AccessUserLevelReq>()).Returns(MakeDto());
            _service.CreateAsync(Arg.Any<AccessUserLevelDto>()).ThrowsAsync(new InvalidOperationException("already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(MakeReq()));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_ServiceCompletes_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            _service.DeleteAsync(1, "dom\\user", 2).Returns(true);

            // Act
            var result = await _controller.Delete(1, "dom%5Cuser", 2);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True(Assert.IsType<bool>(ok.Value));
            await _service.Received(1).DeleteAsync(1, "dom\\user", 2);
        }

        [Fact]
        public async Task Delete_DecodesNtLoginBeforeServiceCall()
        {
            // Arrange
            const string encoded = "dom%5Cjsmith";
            const string decoded = "dom\\jsmith";
            _service.DeleteAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(true);

            // Act
            await _controller.Delete(1, encoded, 10);

            // Assert
            await _service.Received(1).DeleteAsync(1, decoded, 10);
        }

        [Fact]
        public async Task Delete_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _service.DeleteAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>())
                    .ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Delete(99, "unknown", 88));
        }

        #endregion
    }
}
