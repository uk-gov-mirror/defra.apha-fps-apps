using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.Costbook.CostBookMaintenanceApiClientTest
{
    public class CostBookMaintenanceApiClientTests
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly CostBookMaintenanceApiClient _client;

        public CostBookMaintenanceApiClientTests()
        {
            _http = Substitute.For<ICostBookHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new CostBookMaintenanceApiClient(_http, _mapper);
        }

        [Fact]
        public async Task GetSettingsAsync_WhenSuccess_MapsAndReturnsDto()
        {
            var apiResponse = new ApiResponse<MaintenanceSettingsRes> { Success = true, Data = new MaintenanceSettingsRes() };
            var mapped = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(new MaintenanceSettingsDto());
            _http.GetAsync<MaintenanceSettingsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse).Returns(mapped);

            var result = await _client.GetSettingsAsync();

            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateSettingsAsync_OnSuccess_MapsRequestAndReturnsMappedResponse()
        {
            var dto = new MaintenanceSettingsDto { InflationAnimals = 1m };
            var req = new MaintenanceSettingsReq();
            var apiResponse = new ApiResponse<MaintenanceSettingsRes> { Success = true, Data = new MaintenanceSettingsRes() };
            var mapped = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(dto);

            _mapper.Map<MaintenanceSettingsReq>(dto).Returns(req);
            _http.PutAsync<MaintenanceSettingsReq, MaintenanceSettingsRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse).Returns(mapped);

            var result = await _client.UpdateSettingsAsync(dto);

            Assert.True(result.Success);
            await _http.Received(1).PutAsync<MaintenanceSettingsReq, MaintenanceSettingsRes>(Arg.Any<string>(), req);
            _mapper.Received(1).Map<ApiResponseDto<MaintenanceSettingsDto>>(apiResponse);
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WhenSuccess_MapsAndReturnsList()
        {
            var apiResponse = new ApiResponse<List<AccountCategoryMaintenanceRes>> { Success = true, Data = new List<AccountCategoryMaintenanceRes> { new AccountCategoryMaintenanceRes() } };
            var mapped = new List<AccountCategoryMaintenanceDto> { new AccountCategoryMaintenanceDto() };
            _http.GetAsync<List<AccountCategoryMaintenanceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<AccountCategoryMaintenanceDto>>(apiResponse.Data!).Returns(mapped);

            var result = await _client.GetAccountCategoriesAsync();

            Assert.True(result.Success);
            Assert.Same(mapped, result.Data);
            await _http.Received(1).GetAsync<List<AccountCategoryMaintenanceRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPaginatedAccountCategoriesAsync_WhenSuccess_MapsDataAndPagination()
        {
            var apiResponse = new ApiResponse<List<AccountCategoryMaintenanceRes>>
            {
                Success = true,
                Data = new List<AccountCategoryMaintenanceRes> { new AccountCategoryMaintenanceRes() },
                Pagination = new Pagination { PageNumber = 4, PageSize = 25, TotalPages = 5, TotalRecords = 125 }
            };
            var mapped = new List<AccountCategoryMaintenanceDto> { new AccountCategoryMaintenanceDto() };
            _http.GetAsync<List<AccountCategoryMaintenanceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<AccountCategoryMaintenanceDto>>(apiResponse.Data!).Returns(mapped);

            var result = await _client.GetPaginatedAccountCategoriesAsync(new QueryParameters<string> { Page = 4, PageSize = 25 });

            Assert.True(result.Success);
            Assert.Same(mapped, result.Data);
            Assert.NotNull(result.Pagination);
            Assert.Equal(4, result.Pagination!.PageNumber);
            Assert.Equal(25, result.Pagination.PageSize);
            Assert.Equal(5, result.Pagination.TotalPages);
            Assert.Equal(125, result.Pagination.TotalRecords);
        }

        [Fact]
        public async Task GetPaginatedAccountCategoriesAsync_WhenNoPagination_PaginationIsNull()
        {
            var apiResponse = new ApiResponse<List<AccountCategoryMaintenanceRes>> { Success = true, Data = new List<AccountCategoryMaintenanceRes> { new AccountCategoryMaintenanceRes() }, Pagination = null };
            var mapped = new List<AccountCategoryMaintenanceDto> { new AccountCategoryMaintenanceDto() };
            _http.GetAsync<List<AccountCategoryMaintenanceRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<List<AccountCategoryMaintenanceDto>>(apiResponse.Data!).Returns(mapped);

            var result = await _client.GetPaginatedAccountCategoriesAsync(new QueryParameters<string>());

            Assert.True(result.Success);
            Assert.Same(mapped, result.Data);
            Assert.Null(result.Pagination);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_WhenSuccess_MapsRequestAndReturnsMappedResponse()
        {
            var dto = new AccountCategoryMaintenanceDto { AccShortName = "ACC1", Csg7Group = "CSG1" };
            var req = new AccountCategoryMaintenanceReq();
            var apiResponse = new ApiResponse<AccountCategoryMaintenanceRes> { Success = true, Data = new AccountCategoryMaintenanceRes() };
            var mapped = ApiResponseDto<AccountCategoryMaintenanceDto>.SuccessResponse(dto);

            _mapper.Map<AccountCategoryMaintenanceReq>(dto).Returns(req);
            _http.PutAsync<AccountCategoryMaintenanceReq, AccountCategoryMaintenanceRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AccountCategoryMaintenanceDto>>(apiResponse).Returns(mapped);

            var result = await _client.UpdateAccountCategoryAsync(dto.AccShortName!, dto);

            Assert.True(result.Success);
            await _http.Received(1).PutAsync<AccountCategoryMaintenanceReq, AccountCategoryMaintenanceRes>(Arg.Any<string>(), req);
            _mapper.Received(1).Map<ApiResponseDto<AccountCategoryMaintenanceDto>>(apiResponse);
        }
    }
}
