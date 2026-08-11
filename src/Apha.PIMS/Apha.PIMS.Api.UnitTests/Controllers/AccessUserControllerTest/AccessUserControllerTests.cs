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

namespace Apha.PIMS.Api.UnitTests.Controllers.AccessUserControllerTest
{
    public class AccessUserControllerTests
    {
        private readonly IAccessUserService _service;
        private readonly IMapper _mapper;
        private readonly AccessUserController _controller;

        public AccessUserControllerTests()
        {
            _service    = Substitute.For<IAccessUserService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new AccessUserController(_service, _mapper);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static AccessUserDto MakeDto(int systemid = 1, string ntlogin = "DOMAIN\\user1") =>
            new AccessUserDto { SystemId = systemid, NtLogin = ntlogin, UserName = "User One", UserEmail = "user1@example.com" };

        private static AccessUserRes MakeRes(int systemid = 1, string ntlogin = "DOMAIN\\user1") =>
            new AccessUserRes { SystemId = systemid, NtLogin = ntlogin, UserName = "User One", UserEmail = "user1@example.com" };

        private static AccessUserReq MakeReq(int systemid = 1, string ntlogin = "DOMAIN\\user1") =>
            new AccessUserReq { SystemId = systemid, NtLogin = ntlogin, UserName = "User One", UserEmail = "user1@example.com" };

        // ── GetAll ────────────────────────────────────────────────────────────────

        #region GetAll

        [Fact]
        public async Task GetAll_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos    = new List<AccessUserDto> { MakeDto(1, "dom\\u1"), MakeDto(1, "dom\\u2") };
            var resList = new List<AccessUserRes> { MakeRes(1, "dom\\u1"), MakeRes(1, "dom\\u2") };
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<AccessUserRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<AccessUserRes>>(ok.Value);
            Assert.Equal(2, returned.Count);
            await _service.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAll_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos    = new List<AccessUserDto>();
            var resList = new List<AccessUserRes>();
            _service.GetAllAsync().Returns(dtos);
            _mapper.Map<List<AccessUserRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<AccessUserRes>>(ok.Value));
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

        // ── GetPaged ───────────────────────────────────────────────────────────────

        #region GetPaged

        [Fact]
        public async Task GetPaged_ServiceReturnsData_ReturnsOkWithMappedPaginationResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "dom" };
            var dtos = new List<AccessUserDto> { MakeDto(1, "dom\\u1"), MakeDto(1, "dom\\u2") };
            var pagedDto = new PaginatedResult<AccessUserDto>(dtos, new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 });

            var resList = new List<AccessUserRes> { MakeRes(1, "dom\\u1"), MakeRes(1, "dom\\u2") };
            var pageRes = new PaginationRes<AccessUserRes>(resList, new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 });

            _service.GetPagedAsync(query).Returns(pagedDto);
            _mapper.Map<PaginationRes<AccessUserRes>>(pagedDto).Returns(pageRes);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pageRes, ok.Value);
            await _service.Received(1).GetPagedAsync(query);
            _mapper.Received(1).Map<PaginationRes<AccessUserRes>>(pagedDto);
        }

        [Fact]
        public async Task GetPaged_ServiceReturnsEmptyData_ReturnsOkWithEmptyPaginationResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedDto = new PaginatedResult<AccessUserDto>(new List<AccessUserDto>(), new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var pageRes = new PaginationRes<AccessUserRes>(new List<AccessUserRes>(), new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _service.GetPagedAsync(query).Returns(pagedDto);
            _mapper.Map<PaginationRes<AccessUserRes>>(pagedDto).Returns(pageRes);

            // Act
            var result = await _controller.GetPaged(query);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pageRes, ok.Value);
        }

        [Fact]
        public async Task GetPaged_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPagedAsync(query).ThrowsAsync(new Exception("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPaged(query));
        }

        #endregion

        // ── GetBySystemId ─────────────────────────────────────────────────────────

        #region GetBySystemId

        [Fact]
        public async Task GetBySystemId_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos    = new List<AccessUserDto> { MakeDto(2, "dom\\u1") };
            var resList = new List<AccessUserRes> { MakeRes(2, "dom\\u1") };
            _service.GetBySystemIdAsync(2).Returns(dtos);
            _mapper.Map<List<AccessUserRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetBySystemId(2);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Single(Assert.IsType<List<AccessUserRes>>(ok.Value));
            await _service.Received(1).GetBySystemIdAsync(2);
        }

        [Fact]
        public async Task GetBySystemId_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            _service.GetBySystemIdAsync(99).Returns(new List<AccessUserDto>());
            _mapper.Map<List<AccessUserRes>>(Arg.Any<List<AccessUserDto>>()).Returns(new List<AccessUserRes>());

            // Act
            var result = await _controller.GetBySystemId(99);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Empty(Assert.IsType<List<AccessUserRes>>(ok.Value));
        }

        #endregion

        // ── GetById ───────────────────────────────────────────────────────────────

        #region GetById

        [Fact]
        public async Task GetById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            const string encodedLogin = "DOMAIN%5Cuser1";
            const string decodedLogin = "DOMAIN\\user1";
            var dto = MakeDto(1, decodedLogin);
            var res = MakeRes(1, decodedLogin);
            _service.GetByIdAsync(1, decodedLogin).Returns(dto);
            _mapper.Map<AccessUserRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetById(1, encodedLogin);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetByIdAsync(1, decodedLogin);
        }

        [Fact]
        public async Task GetById_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            _service.GetByIdAsync(Arg.Any<int>(), Arg.Any<string>()).Returns((AccessUserDto?)null);

            // Act
            var result = await _controller.GetById(99, "unknown");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_DecodesNtLoginBeforeServiceCall()
        {
            // Arrange — encoded backslash %5C should be decoded to \ before service lookup
            const string encodedLogin = "dom%5Cjsmith";
            const string decodedLogin = "dom\\jsmith";
            _service.GetByIdAsync(1, decodedLogin).Returns(MakeDto(1, decodedLogin));
            _mapper.Map<AccessUserRes>(Arg.Any<AccessUserDto>()).Returns(MakeRes(1, decodedLogin));

            // Act
            await _controller.GetById(1, encodedLogin);

            // Assert
            await _service.Received(1).GetByIdAsync(1, decodedLogin);
        }

        #endregion

        // ── Create ────────────────────────────────────────────────────────────────

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtActionWithMappedResult()
        {
            // Arrange
            var req     = MakeReq();
            var dto     = MakeDto();
            var created = MakeDto(1, "dom\\user");
            var res     = MakeRes(1, "dom\\user");
            _mapper.Map<AccessUserDto>(req).Returns(dto);
            _service.CreateAsync(dto).Returns(created);
            _mapper.Map<AccessUserRes>(created).Returns(res);

            // Act
            var result = await _controller.Create(req);

            // Assert
            var created201 = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(AccessUserController.GetById), created201.ActionName);
            Assert.Equal(res, created201.Value);
        }

        [Fact]
        public async Task Create_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            // Arrange — duplicate user guard
            _mapper.Map<AccessUserDto>(Arg.Any<AccessUserReq>()).Returns(MakeDto());
            _service.CreateAsync(Arg.Any<AccessUserDto>()).ThrowsAsync(new InvalidOperationException("already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(MakeReq()));
        }

        #endregion

        // ── Update ────────────────────────────────────────────────────────────────

        #region Update

        [Fact]
        public async Task Update_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            const string encodedLogin = "dom%5Cuser";
            const string decodedLogin = "dom\\user";
            var dto     = MakeDto(1, decodedLogin);
            var updated = MakeDto(1, decodedLogin);
            var res     = MakeRes(1, decodedLogin);
            _mapper.Map<AccessUserDto>(Arg.Any<AccessUserReq>()).Returns(dto);
            _service.UpdateAsync(Arg.Any<AccessUserDto>()).Returns(updated);
            _mapper.Map<AccessUserRes>(updated).Returns(res);

            // Act
            var result = await _controller.Update(1, encodedLogin, MakeReq());

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task Update_SetsCompositePkOnDtoBeforeCallingService()
        {
            // Arrange — controller should set SystemId and NtLogin from route on the mapped DTO
            const string decodedLogin = "dom\\user";
            var dto = new AccessUserDto { SystemId = 0, NtLogin = "" }; // blank from mapper
            _mapper.Map<AccessUserDto>(Arg.Any<AccessUserReq>()).Returns(dto);
            _service.UpdateAsync(Arg.Any<AccessUserDto>()).Returns(MakeDto(5, decodedLogin));
            _mapper.Map<AccessUserRes>(Arg.Any<AccessUserDto>()).Returns(MakeRes(5, decodedLogin));

            // Act
            await _controller.Update(5, "dom%5Cuser", MakeReq());

            // Assert — dto should have route values set
            await _service.Received(1).UpdateAsync(
                Arg.Is<AccessUserDto>(d => d.SystemId == 5 && d.NtLogin == decodedLogin));
        }

        [Fact]
        public async Task Update_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _mapper.Map<AccessUserDto>(Arg.Any<AccessUserReq>()).Returns(MakeDto());
            _service.UpdateAsync(Arg.Any<AccessUserDto>()).ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Update(99, "unknown", MakeReq()));
        }

        #endregion

        // ── Delete ────────────────────────────────────────────────────────────────

        #region Delete

        [Fact]
        public async Task Delete_ServiceCompletes_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            _service.DeleteAsync(1, "dom\\user").Returns(true);

            // Act
            var result = await _controller.Delete(1, "dom%5Cuser");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True(Assert.IsType<bool>(ok.Value));
            await _service.Received(1).DeleteAsync(1, "dom\\user");
        }

        [Fact]
        public async Task Delete_DecodesNtLoginBeforeServiceCall()
        {
            // Arrange
            const string encoded = "dom%5Cjsmith";
            const string decoded = "dom\\jsmith";
            _service.DeleteAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(true);

            // Act
            await _controller.Delete(1, encoded);

            // Assert
            await _service.Received(1).DeleteAsync(1, decoded);
        }

        [Fact]
        public async Task Delete_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _service.DeleteAsync(Arg.Any<int>(), Arg.Any<string>())
                    .ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.Delete(99, "unknown"));
        }

        #endregion
    }
}
