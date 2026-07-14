using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Api.Controllers;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Api.UnitTests.Controllers.AccountGroupControllerTest
{
    public class AccountGroupControllerTests
    {
        private readonly IAccountGroupService _service;
        private readonly IMapper _mapper;
        private readonly AccountGroupController _controller;

        public AccountGroupControllerTests()
        {
            _service = Substitute.For<IAccountGroupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AccountGroupController(_service, _mapper);
        }

        // ── GetAllAccountGroups ───────────────────────────────────────────────

        #region GetAllAccountGroups Tests

        [Fact]
        public async Task GetAllAccountGroups_ServiceReturnsList_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<AccountGroupDto>
            {
                new AccountGroupDto { Csg7group = "CSG001", Useinflation = true },
                new AccountGroupDto { Csg7group = "CSG002", Useinflation = false }
            };
            var resList = new List<AccountGroupRes>
            {
                new AccountGroupRes { Csg7Group = "CSG001", UseInflation = true },
                new AccountGroupRes { Csg7Group = "CSG002", UseInflation = false }
            };
            _service.GetAllAccountGroupAsync().Returns(dtos);
            _mapper.Map<List<AccountGroupRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllAccountGroups();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(resList, okResult.Value);
            await _service.Received(1).GetAllAccountGroupAsync();
        }

        [Fact]
        public async Task GetAllAccountGroups_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<AccountGroupDto>();
            var resList = new List<AccountGroupRes>();
            _service.GetAllAccountGroupAsync().Returns(dtos);
            _mapper.Map<List<AccountGroupRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllAccountGroups();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(resList, okResult.Value);
        }

        #endregion

        // ── GetPaginatedAccountGroups ─────────────────────────────────────────

        #region GetPaginatedAccountGroups Tests

        [Fact]
        public async Task GetPaginatedAccountGroups_ValidQuery_ReturnsOkWithPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PaginatedResult<AccountGroupDto>(
                new List<AccountGroupDto>
                {
                    new AccountGroupDto { Csg7group = "CSG001", Useinflation = true }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 });
            var pagedRes = new PaginationRes<AccountGroupRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _service.GetPaginatedAsync(queryParams).Returns(pagedData);
            _mapper.Map<PaginationRes<AccountGroupRes>>(pagedData).Returns(pagedRes);

            // Act
            var result = await _controller.GetPaginatedAccountGroups(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(pagedRes, okResult.Value);
            await _service.Received(1).GetPaginatedAsync(queryParams);
        }

        #endregion

        // ── GetAccountGroup ───────────────────────────────────────────────────

        #region GetAccountGroup Tests

        [Fact]
        public async Task GetAccountGroup_ExistingKey_ReturnsOkWithMappedRes()
        {
            // Arrange
            var key = "CSG001";
            var dto = new AccountGroupDto { Csg7group = key, Useinflation = true };
            var res = new AccountGroupRes { Csg7Group = key, UseInflation = true };
            _service.GetByCsg7GroupAsync(key).Returns(dto);
            _mapper.Map<AccountGroupRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetAccountGroup(key);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _service.Received(1).GetByCsg7GroupAsync(key);
        }

        [Fact]
        public async Task GetAccountGroup_NonExistentKey_ReturnsNotFound()
        {
            // Arrange
            var key = "NOTEXIST";
            _service.GetByCsg7GroupAsync(key).Returns((AccountGroupDto?)null);

            // Act
            var result = await _controller.GetAccountGroup(key);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        // ── AddAccountGroup ───────────────────────────────────────────────────

        #region AddAccountGroup Tests

        [Fact]
        public async Task AddAccountGroup_ValidRequest_ReturnsCreatedAtActionWithMappedRes()
        {
            // Arrange
            var req = new AccountGroupReq { Csg7Group = "CSG003", UseInflation = true };
            var dto = new AccountGroupDto { Csg7group = "CSG003", Useinflation = true };
            var created = new AccountGroupDto { Csg7group = "CSG003", Useinflation = true };
            var res = new AccountGroupRes { Csg7Group = "CSG003", UseInflation = true };
            _mapper.Map<AccountGroupDto>(req).Returns(dto);
            _service.AddAccountGroupAsync(dto).Returns(created);
            _mapper.Map<AccountGroupRes>(created).Returns(res);

            // Act
            var result = await _controller.AddAccountGroup(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetAccountGroup), createdResult.ActionName);
            Assert.Same(res, createdResult.Value);
            await _service.Received(1).AddAccountGroupAsync(dto);
        }

        [Fact]
        public async Task AddAccountGroup_DuplicateKey_PropagatesArgumentException()
        {
            // Arrange
            var req = new AccountGroupReq { Csg7Group = "CSG001", UseInflation = true };
            var dto = new AccountGroupDto { Csg7group = "CSG001", Useinflation = true };
            _mapper.Map<AccountGroupDto>(req).Returns(dto);
            _service.AddAccountGroupAsync(dto).Throws(new ArgumentException("AccountGroup 'CSG001' already exists."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.AddAccountGroup(req));
        }

        #endregion

        // ── UpdateAccountGroup ────────────────────────────────────────────────

        #region UpdateAccountGroup Tests

        [Fact]
        public async Task UpdateAccountGroup_ValidRequest_ReturnsOkWithUpdatedRes()
        {
            // Arrange
            var key = "CSG001";
            var req = new AccountGroupReq { Csg7Group = key, UseInflation = false };
            var dto = new AccountGroupDto { Csg7group = key, Useinflation = false };
            var updated = new AccountGroupDto { Csg7group = key, Useinflation = false };
            var res = new AccountGroupRes { Csg7Group = key, UseInflation = false };
            _mapper.Map<AccountGroupDto>(req).Returns(dto);
            _service.UpdateAccountGroupAsync(key, dto).Returns(updated);
            _mapper.Map<AccountGroupRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateAccountGroup(key, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _service.Received(1).UpdateAccountGroupAsync(key, dto);
        }

        [Fact]
        public async Task UpdateAccountGroup_NonExistentKey_PropagatesKeyNotFoundException()
        {
            // Arrange
            var key = "NOTEXIST";
            var req = new AccountGroupReq { Csg7Group = key, UseInflation = true };
            var dto = new AccountGroupDto { Csg7group = key, Useinflation = true };
            _mapper.Map<AccountGroupDto>(req).Returns(dto);
            _service.UpdateAccountGroupAsync(key, dto).Throws(new KeyNotFoundException($"AccountGroup '{key}' not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateAccountGroup(key, req));
        }

        #endregion

        // ── DeleteAccountGroup ────────────────────────────────────────────────

        #region DeleteAccountGroup Tests

        [Fact]
        public async Task DeleteAccountGroup_ExistingKey_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var key = "CSG001";
            _service.DeleteAccountGroupAsync(key).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAccountGroup(key);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value!;
            var successProp = value.GetType().GetProperty("success");
            Assert.NotNull(successProp);
            Assert.True((bool)successProp.GetValue(value)!);
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            Assert.Equal("Deleted successfully", (string)messageProp.GetValue(value)!);
            await _service.Received(1).DeleteAccountGroupAsync(key);
        }

        [Fact]
        public async Task DeleteAccountGroup_NonExistentKey_PropagatesKeyNotFoundException()
        {
            // Arrange
            var key = "NOTEXIST";
            _service.DeleteAccountGroupAsync(key).Throws(new KeyNotFoundException($"AccountGroup '{key}' not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteAccountGroup(key));
        }

        [Fact]
        public async Task DeleteAccountGroup_WhitespaceKey_PropagatesArgumentException()
        {
            // Arrange — controller guard throws before calling service for blank Csg7group
            var key = "   ";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAccountGroup(key));
            await _service.DidNotReceive().DeleteAccountGroupAsync(Arg.Any<string>());
        }

        #endregion
    }
}
