using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.UnitTests.Services.Costbook.CostBookAccountGroupServiceTest
{
    public class CostBookAccountGroupServiceTests
    {
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookAccountGroupApiClient _accountGroupApi;
        private readonly CostBookAccountGroupService _service;

        public CostBookAccountGroupServiceTests()
        {
            _costBookClient = Substitute.For<ICostBookApiClient>();
            _accountGroupApi = Substitute.For<ICostBookAccountGroupApiClient>();
            _costBookClient.CostbookAccountGroup.Returns(_accountGroupApi);
            _service = new CostBookAccountGroupService(_costBookClient);
        }

        [Fact]
        public async Task GetAllAccountGroupsAsync_ForwardsCallAndReturnsResponse()
        {
            var expected = ApiResponseDto<List<AccountGroupDto>>.SuccessResponse(new List<AccountGroupDto>
            {
                new AccountGroupDto { Csg7Group = "CSG001" }
            });

            _accountGroupApi.GetAllAccountGroupsAsync().Returns(expected);

            var result = await _service.GetAllAccountGroupsAsync();

            Assert.Same(expected, result);
            await _accountGroupApi.Received(1).GetAllAccountGroupsAsync();
        }

        [Fact]
        public async Task GetPaginatedAccountGroupsAsync_ForwardsQueryAndReturnsResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "x" };
            var expected = ApiResponseDto<List<AccountGroupDto>>.SuccessResponse(new List<AccountGroupDto>());

            _accountGroupApi.GetPaginatedAccountGroupsAsync(query).Returns(expected);

            var result = await _service.GetPaginatedAccountGroupsAsync(query);

            Assert.Same(expected, result);
            await _accountGroupApi.Received(1).GetPaginatedAccountGroupsAsync(
                Arg.Is<QueryParameters<string>>(q => q.Page == 1 && q.PageSize == 10 && q.Search == "x"));
        }

        [Fact]
        public async Task GetAccountGroupAsync_ForwardsKeyAndReturnsResponse()
        {
            var key = "CSG001";
            var expected = ApiResponseDto<AccountGroupDto>.SuccessResponse(new AccountGroupDto { Csg7Group = key });

            _accountGroupApi.GetAccountGroupAsync(key).Returns(expected);

            var result = await _service.GetAccountGroupAsync(key);

            Assert.Same(expected, result);
            await _accountGroupApi.Received(1).GetAccountGroupAsync(key);
        }

        [Fact]
        public async Task AddUpdateDeleteAccountGroupAsync_ForwardsCallsAndReturnsResponses()
        {
            var dto = new AccountGroupDto { Csg7Group = "CSG_NEW" };

            var addResp = ApiResponseDto<AccountGroupDto>.SuccessResponse(dto);
            _accountGroupApi.AddAccountGroupAsync(dto).Returns(addResp);
            var addResult = await _service.AddAccountGroupAsync(dto);
            Assert.Same(addResp, addResult);
            await _accountGroupApi.Received(1).AddAccountGroupAsync(dto);

            var updateResp = ApiResponseDto<AccountGroupDto>.SuccessResponse(dto);
            _accountGroupApi.UpdateAccountGroupAsync(dto.Csg7Group!, dto).Returns(updateResp);
            var updateResult = await _service.UpdateAccountGroupAsync(dto.Csg7Group!, dto);
            Assert.Same(updateResp, updateResult);
            await _accountGroupApi.Received(1).UpdateAccountGroupAsync(dto.Csg7Group!, dto);

            var deleteResp = ApiResponseDto<bool>.SuccessResponse(true);
            _accountGroupApi.DeleteAccountGroupAsync(dto.Csg7Group!).Returns(deleteResp);
            var delResult = await _service.DeleteAccountGroupAsync(dto.Csg7Group!);
            Assert.Same(deleteResp, delResult);
            await _accountGroupApi.Received(1).DeleteAccountGroupAsync(dto.Csg7Group!);
        }
    }
}
