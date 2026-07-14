using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.UnitTests.Services.Costbook.CostBookCapsStaffServiceTest
{
    public class CostBookCapsStaffServiceTests
    {
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookCapsStaffApiClient _capsStaffApi;
        private readonly CostBookCapsStaffService _service;

        public CostBookCapsStaffServiceTests()
        {
            _costBookClient = Substitute.For<ICostBookApiClient>();
            _capsStaffApi = Substitute.For<ICostBookCapsStaffApiClient>();
            _costBookClient.CostbookCapsStaff.Returns(_capsStaffApi);
            _service = new CostBookCapsStaffService(_costBookClient);
        }

        [Fact]
        public async Task GetPaginatedCapsStaffAsync_ForwardsQueryAndReturnsResponse()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var expected = ApiResponseDto<List<StaffDto>>.SuccessResponse(new List<StaffDto>());

            _capsStaffApi.GetPaginatedCapsStaffAsync(query).Returns(expected);

            var result = await _service.GetPaginatedCapsStaffAsync(query);

            Assert.Same(expected, result);
            await _capsStaffApi.Received(1).GetPaginatedCapsStaffAsync(
                Arg.Is<QueryParameters<string>>(q => q.Page == 2 && q.PageSize == 5));
        }

        [Fact]
        public async Task GetCapsStaffByMNumberAsync_ForwardsMNumberAndReturnsResponse()
        {
            var mNumber = "M123";
            var expected = ApiResponseDto<StaffDto>.SuccessResponse(new StaffDto { Mnumber = mNumber });

            _capsStaffApi.GetCapsStaffByMNumberAsync(mNumber).Returns(expected);

            var result = await _service.GetCapsStaffByMNumberAsync(mNumber);

            Assert.Same(expected, result);
            await _capsStaffApi.Received(1).GetCapsStaffByMNumberAsync(mNumber);
        }

        [Fact]
        public async Task AddUpdateDeleteCapsStaffAsync_ForwardsCallsAndReturnsResponses()
        {
            var dto = new StaffDto { Mnumber = "M999" };

            var addResp = ApiResponseDto<StaffDto>.SuccessResponse(dto);
            _capsStaffApi.AddCapsStaffAsync(dto).Returns(addResp);
            var addResult = await _service.AddCapsStaffAsync(dto);
            Assert.Same(addResp, addResult);
            await _capsStaffApi.Received(1).AddCapsStaffAsync(dto);

            var updateResp = ApiResponseDto<StaffDto>.SuccessResponse(dto);
            _capsStaffApi.UpdateCapsStaffAsync(dto.Mnumber!, dto).Returns(updateResp);
            var updateResult = await _service.UpdateCapsStaffAsync(dto.Mnumber!, dto);
            Assert.Same(updateResp, updateResult);
            await _capsStaffApi.Received(1).UpdateCapsStaffAsync(dto.Mnumber!, dto);

            var deleteResp = ApiResponseDto<bool>.SuccessResponse(true);
            _capsStaffApi.DeleteCapsStaffAsync(dto.Mnumber!).Returns(deleteResp);
            var delResult = await _service.DeleteCapsStaffAsync(dto.Mnumber!);
            Assert.Same(deleteResp, delResult);
            await _capsStaffApi.Received(1).DeleteCapsStaffAsync(dto.Mnumber!);
        }
    }
}
