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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.Costbook.CostBookCapsStaffApiClientTest
{
    public class CostBookCapsStaffApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookCapsStaffApiClient _client;

        public CostBookCapsStaffApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookCapsStaffApiClient(_http, _mapper);
        }

        [Fact]
        public async Task GetPaginatedCapsStaffAsync_WhenSuccess_MapsDataAndPagination()
        {
            var apiResponse = new ApiResponse<List<StaffRes>>
            {
                Success = true,
                Data = new List<StaffRes> { new StaffRes() },
                Pagination = new Pagination { PageNumber = 3, PageSize = 7, TotalPages = 4, TotalRecords = 28 }
            };
            var mapped = new List<StaffDto> { new StaffDto() };
            _http.GetAsync<List<StaffRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<StaffDto>>(apiResponse.Data!).Returns(mapped);

            var result = await _client.GetPaginatedCapsStaffAsync(new QueryParameters<string> { Page = 3, PageSize = 7 });

            Assert.True(result.Success);
            Assert.Same(mapped, result.Data);
            Assert.NotNull(result.Pagination);
            Assert.Equal(3, result.Pagination!.PageNumber);
            Assert.Equal(7, result.Pagination.PageSize);
            Assert.Equal(4, result.Pagination.TotalPages);
            Assert.Equal(28, result.Pagination.TotalRecords);
        }

        [Fact]
        public async Task GetPaginatedCapsStaffAsync_WhenNoPagination_PaginationIsNull()
        {
            var apiResponse = new ApiResponse<List<StaffRes>> { Success = true, Data = new List<StaffRes> { new StaffRes() }, Pagination = null };
            var mapped = new List<StaffDto> { new StaffDto() };
            _http.GetAsync<List<StaffRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<StaffDto>>(apiResponse.Data!).Returns(mapped);

            var result = await _client.GetPaginatedCapsStaffAsync(new QueryParameters<string>());

            Assert.True(result.Success);
            Assert.Same(mapped, result.Data);
            Assert.Null(result.Pagination);
        }

        [Fact]
        public async Task GetCapsStaffByMNumberAsync_WhenSuccess_MapsUsingMapper()
        {
            var apiResponse = new ApiResponse<StaffRes> { Success = true, Data = new StaffRes() };
            var mapped = ApiResponseDto<StaffDto>.SuccessResponse(new StaffDto { Mnumber = "M1" });
            _http.GetAsync<StaffRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffDto>>(apiResponse).Returns(mapped);

            var result = await _client.GetCapsStaffByMNumberAsync("M/1");

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<StaffRes>(Arg.Is<string>(s => s.Contains("M%2F1") || s.Contains("M%2f1")));
            _mapper.Received(1).Map<ApiResponseDto<StaffDto>>(apiResponse);
        }

        [Fact]
        public async Task AddUpdateDeleteCapsStaffAsync_MapsRequestsAndResponses()
        {
            var dto = new StaffDto { Mnumber = "M100" };
            var req = new StaffReq();
            var addResponse = new ApiResponse<StaffRes> { Success = true, Data = new StaffRes() };
            var mappedAdd = ApiResponseDto<StaffDto>.SuccessResponse(dto);

            _mapper.Map<StaffReq>(dto).Returns(req);
            _http.PostAsync<StaffReq, StaffRes>(Arg.Any<string>(), req).Returns(addResponse);
            _mapper.Map<ApiResponseDto<StaffDto>>(addResponse).Returns(mappedAdd);

            var addResult = await _client.AddCapsStaffAsync(dto);
            Assert.True(addResult.Success);
            await _http.Received(1).PostAsync<StaffReq, StaffRes>(Arg.Any<string>(), req);

            var updateResponse = new ApiResponse<StaffRes> { Success = true, Data = new StaffRes() };
            _http.PutAsync<StaffReq, StaffRes>(Arg.Any<string>(), req).Returns(updateResponse);
            var mappedUpdate = ApiResponseDto<StaffDto>.SuccessResponse(dto);
            _mapper.Map<ApiResponseDto<StaffDto>>(updateResponse).Returns(mappedUpdate);

            var updateResult = await _client.UpdateCapsStaffAsync(dto.Mnumber!, dto);
            Assert.True(updateResult.Success);
            await _http.Received(1).PutAsync<StaffReq, StaffRes>(Arg.Any<string>(), req);

            _http.DeleteAsync<object>(Arg.Any<string>()).Returns(new ApiResponse<object> { Success = true });
            var deleteResult = await _client.DeleteCapsStaffAsync(dto.Mnumber!);
            Assert.True(deleteResult.Success);
            await _http.Received(1).DeleteAsync<object>(Arg.Any<string>());
        }

        [Fact]
        public async Task WhenHttpThrows_ReturnsFailureWithInternalCode()
        {
            _http.GetAsync<List<StaffRes>>(Arg.Any<string>()).Throws(new Exception("boom"));

            var result = await _client.GetPaginatedCapsStaffAsync(new QueryParameters<string>());

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }
    }
}
