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
using Xunit;

namespace Apha.Costbook.Api.UnitTests.Controllers.CapsStaffControllerTest
{
    public class CapsStaffControllerTests
    {
        private readonly ICapsStaffService _service;
        private readonly IMapper _mapper;
        private readonly CapsStaffController _controller;

        public CapsStaffControllerTests()
        {
            _service = Substitute.For<ICapsStaffService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new CapsStaffController(_service, _mapper);
        }

        // ── GetAllCapsStaff ───────────────────────────────────────────────────

        #region GetAllCapsStaff Tests

        [Fact]
        public async Task GetAllCapsStaff_ServiceReturnsList_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<StaffDto>
            {
                new StaffDto { Mnumber = "M001", Name = "Alice" },
                new StaffDto { Mnumber = "M002", Name = "Bob" }
            };
            var resList = new List<StaffRes>
            {
                new StaffRes { Mnumber = "M001", Name = "Alice" },
                new StaffRes { Mnumber = "M002", Name = "Bob" }
            };
            _service.GetAllStaffAsync().Returns(dtos);
            _mapper.Map<List<StaffRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllCapsStaff();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(resList, okResult.Value);
            await _service.Received(1).GetAllStaffAsync();
        }

        [Fact]
        public async Task GetAllCapsStaff_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<StaffDto>();
            var resList = new List<StaffRes>();
            _service.GetAllStaffAsync().Returns(dtos);
            _mapper.Map<List<StaffRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAllCapsStaff();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(resList, okResult.Value);
        }

        #endregion

        // ── GetPaginatedCapsStaff ─────────────────────────────────────────────

        #region GetPaginatedCapsStaff Tests

        [Fact]
        public async Task GetPaginatedCapsStaff_ValidQuery_ReturnsOkWithPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PaginatedResult<StaffDto>(
                new List<StaffDto> { new StaffDto { Mnumber = "M001", Name = "Alice" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            var pagedRes = new PaginationRes<StaffRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _service.GetPaginatedAsync(queryParams).Returns(pagedData);
            _mapper.Map<PaginationRes<StaffRes>>(pagedData).Returns(pagedRes);

            // Act
            var result = await _controller.GetPaginatedCapsStaff(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(pagedRes, okResult.Value);
            await _service.Received(1).GetPaginatedAsync(queryParams);
        }

        #endregion

        // ── GetCapsStaff ──────────────────────────────────────────────────────

        #region GetCapsStaff Tests

        [Fact]
        public async Task GetCapsStaff_ExistingMnumber_ReturnsOkWithMappedRes()
        {
            // Arrange
            var Mnumber = "M001";
            var dto = new StaffDto { Mnumber = Mnumber, Name = "Alice" };
            var res = new StaffRes { Mnumber = Mnumber, Name = "Alice" };
            _service.GetByMNumberAsync(Mnumber).Returns(dto);
            _mapper.Map<StaffRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetCapsStaff(Mnumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _service.Received(1).GetByMNumberAsync(Mnumber);
        }

        [Fact]
        public async Task GetCapsStaff_NonExistentMnumber_ReturnsNotFound()
        {
            // Arrange
            var Mnumber = "NOTEXIST";
            _service.GetByMNumberAsync(Mnumber).Returns((StaffDto?)null);

            // Act
            var result = await _controller.GetCapsStaff(Mnumber);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        // ── AddCapsStaff ──────────────────────────────────────────────────────

        #region AddCapsStaff Tests

        [Fact]
        public async Task AddCapsStaff_ValidRequest_ReturnsCreatedAtActionWithMappedRes()
        {
            // Arrange
            var req = new StaffReq { MNumber = "M003", Name = "Charlie" };
            var dto = new StaffDto { Mnumber = "M003", Name = "Charlie" };
            var created = new StaffDto { Mnumber = "M003", Name = "Charlie" };
            var res = new StaffRes { Mnumber = "M003", Name = "Charlie" };
            _mapper.Map<StaffDto>(req).Returns(dto);
            _service.AddStaffAsync(dto).Returns(created);
            _mapper.Map<StaffRes>(created).Returns(res);

            // Act
            var result = await _controller.AddCapsStaff(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetCapsStaff), createdResult.ActionName);
            Assert.Same(res, createdResult.Value);
            await _service.Received(1).AddStaffAsync(dto);
        }

        [Fact]
        public async Task AddCapsStaff_DuplicateMnumber_PropagatesArgumentException()
        {
            // Arrange
            var req = new StaffReq { MNumber = "M001", Name = "Duplicate" };
            var dto = new StaffDto { Mnumber = "M001", Name = "Duplicate" };
            _mapper.Map<StaffDto>(req).Returns(dto);
            _service.AddStaffAsync(dto).Throws(new ArgumentException("A CAPS staff member with Mnumber 'M001' already exists."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.AddCapsStaff(req));
        }

        #endregion

        // ── UpdateCapsStaff ───────────────────────────────────────────────────

        #region UpdateCapsStaff Tests

        [Fact]
        public async Task UpdateCapsStaff_ValidRequest_ReturnsOkWithUpdatedRes()
        {
            // Arrange
            var Mnumber = "M001";
            var req = new StaffReq { MNumber = Mnumber, Name = "Alice Updated" };
            var dto = new StaffDto { Mnumber = Mnumber, Name = "Alice Updated" };
            var updated = new StaffDto { Mnumber = Mnumber, Name = "Alice Updated" };
            var res = new StaffRes { Mnumber = Mnumber, Name = "Alice Updated" };
            _mapper.Map<StaffDto>(req).Returns(dto);
            _service.UpdateStaffAsync(Mnumber, dto).Returns(updated);
            _mapper.Map<StaffRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateCapsStaff(Mnumber, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _service.Received(1).UpdateStaffAsync(Mnumber, dto);
        }

        [Fact]
        public async Task UpdateCapsStaff_NonExistentMnumber_PropagatesKeyNotFoundException()
        {
            // Arrange
            var Mnumber = "NOTEXIST";
            var req = new StaffReq { MNumber = Mnumber, Name = "Ghost" };
            var dto = new StaffDto { Mnumber = Mnumber, Name = "Ghost" };
            _mapper.Map<StaffDto>(req).Returns(dto);
            _service.UpdateStaffAsync(Mnumber, dto).Throws(new KeyNotFoundException($"CAPS staff member '{Mnumber}' not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateCapsStaff(Mnumber, req));
        }

        #endregion

        // ── DeleteCapsStaff ───────────────────────────────────────────────────

        #region DeleteCapsStaff Tests

        [Fact]
        public async Task DeleteCapsStaff_ExistingMnumber_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var Mnumber = "M001";
            _service.DeleteStaffAsync(Mnumber).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteCapsStaff(Mnumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value!;
            var successProp = value.GetType().GetProperty("success");
            Assert.NotNull(successProp);
            Assert.True((bool)successProp.GetValue(value)!);
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            Assert.Equal("Deleted successfully", (string)messageProp.GetValue(value)!);
            await _service.Received(1).DeleteStaffAsync(Mnumber);
        }

        [Fact]
        public async Task DeleteCapsStaff_NonExistentMnumber_PropagatesKeyNotFoundException()
        {
            // Arrange
            var Mnumber = "NOTEXIST";
            _service.DeleteStaffAsync(Mnumber).Throws(new KeyNotFoundException($"CAPS staff member '{Mnumber}' not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteCapsStaff(Mnumber));
        }

        [Fact]
        public async Task DeleteCapsStaff_WhitespaceMnumber_PropagatesArgumentException()
        {
            // Arrange — controller throws ArgumentException before calling the service for blank Mnumber
            var Mnumber = "   ";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteCapsStaff(Mnumber));
            await _service.DidNotReceive().DeleteStaffAsync(Arg.Any<string>());
        }

        #endregion
    }
}
