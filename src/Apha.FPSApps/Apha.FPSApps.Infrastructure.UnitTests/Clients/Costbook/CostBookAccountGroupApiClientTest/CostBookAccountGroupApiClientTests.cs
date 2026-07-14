using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.Costbook.CostBookAccountGroupApiClientTest
{
    public class CostBookAccountGroupApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookAccountGroupApiClient _client;

        public CostBookAccountGroupApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookAccountGroupApiClient(_http, _mapper);
        }

        [Fact]
        public async Task GetAllAccountGroupsAsync_WhenApiReturnsSuccess_MapsAndReturnsDtoList()
        {
            var apiResponse = new ApiResponse<List<AccountGroupRes>> { Success = true, Data = new List<AccountGroupRes> { new AccountGroupRes() } };
            var mapped = new List<AccountGroupDto> { new AccountGroupDto() };
            _http.GetAsync<List<AccountGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<AccountGroupDto>>(apiResponse.Data!).Returns(mapped);

            var result = await _client.GetAllAccountGroupsAsync();

            Assert.True(result.Success);
            Assert.Same(mapped, result.Data);
            await _http.Received(1).GetAsync<List<AccountGroupRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<List<AccountGroupDto>>(apiResponse.Data);
        }

        [Fact]
        public async Task GetPaginatedAccountGroupsAsync_WhenApiReturnsPagination_MapsDataAndPagination()
        {
            var apiResponse = new ApiResponse<List<AccountGroupRes>>
            {
                Success = true,
                Data = new List<AccountGroupRes> { new AccountGroupRes() },
                Pagination = new Pagination { PageNumber = 2, PageSize = 5, TotalPages = 3, TotalRecords = 15 }
            };
            var mapped = new List<AccountGroupDto> { new AccountGroupDto() };
            _http.GetAsync<List<AccountGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<AccountGroupDto>>(apiResponse.Data!).Returns(mapped);

            var result = await _client.GetPaginatedAccountGroupsAsync(new QueryParameters<string> { Page = 2, PageSize = 5 });

            Assert.True(result.Success);
            Assert.Same(mapped, result.Data);
            Assert.NotNull(result.Pagination);
            Assert.Equal(2, result.Pagination!.PageNumber);
            Assert.Equal(5, result.Pagination.PageSize);
            Assert.Equal(3, result.Pagination.TotalPages);
            Assert.Equal(15, result.Pagination.TotalRecords);
        }

        [Fact]
        public async Task GetPaginatedAccountGroupsAsync_WhenApiReturnsNoPagination_PaginationIsNull()
        {
            var apiResponse = new ApiResponse<List<AccountGroupRes>>
            {
                Success = true,
                Data = new List<AccountGroupRes> { new AccountGroupRes() },
                Pagination = null
            };
            var mapped = new List<AccountGroupDto> { new AccountGroupDto() };
            _http.GetAsync<List<AccountGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<AccountGroupDto>>(apiResponse.Data!).Returns(mapped);

            var result = await _client.GetPaginatedAccountGroupsAsync(new QueryParameters<string>());

            Assert.True(result.Success);
            Assert.Same(mapped, result.Data);
            Assert.Null(result.Pagination);
        }

        [Fact]
        public async Task GetAccountGroupAsync_WhenApiReturnsSuccess_MapperCalledWithApiResponse()
        {
            var apiResponse = new ApiResponse<AccountGroupRes> { Success = true, Data = new AccountGroupRes() };
            var mappedResponse = ApiResponseDto<AccountGroupDto>.SuccessResponse(new AccountGroupDto());
            _http.GetAsync<AccountGroupRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountGroupDto>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetAccountGroupAsync("CSG/1");

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<AccountGroupRes>(Arg.Is<string>(s => s.Contains("CSG%2F1") || s.Contains("CSG%2f1")));
            _mapper.Received(1).Map<ApiResponseDto<AccountGroupDto>>(apiResponse);
        }

        [Fact]
        public async Task AddUpdateDeleteAccountGroupAsync_OnSuccess_MapsAndReturnsMappedResponse()
        {
            var dto = new AccountGroupDto { Csg7Group = "CSG001" };
            var req = new AccountGroupReq();
            var apiResponse = new ApiResponse<AccountGroupRes> { Success = true, Data = new AccountGroupRes() };
            var mapped = ApiResponseDto<AccountGroupDto>.SuccessResponse(dto);

            _mapper.Map<AccountGroupReq>(dto).Returns(req);
            _http.PostAsync<AccountGroupReq, AccountGroupRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountGroupDto>>(apiResponse).Returns(mapped);

            var addResult = await _client.AddAccountGroupAsync(dto);
            Assert.True(addResult.Success);
            await _http.Received(1).PostAsync<AccountGroupReq, AccountGroupRes>(Arg.Any<string>(), req);

            _http.PutAsync<AccountGroupReq, AccountGroupRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountGroupDto>>(apiResponse).Returns(mapped);
            var updateResult = await _client.UpdateAccountGroupAsync(dto.Csg7Group!, dto);
            Assert.True(updateResult.Success);
            await _http.Received(1).PutAsync<AccountGroupReq, AccountGroupRes>(Arg.Any<string>(), req);

            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(new ApiResponse<object> { Success = true });
            var delResult = await _client.DeleteAccountGroupAsync(dto.Csg7Group!);
            Assert.True(delResult.Success);
            await _http.Received(1).DeleteAsync<object>(Arg.Any<string>());
        }

        [Fact]
        public async Task Methods_OnHttpException_ReturnsFailureWithInternalError()
        {
            _http.GetAsync<List<AccountGroupRes>>(Arg.Any<string>()).Throws(new Exception("boom"));

            var result = await _client.GetAllAccountGroupsAsync();

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }
    }
}