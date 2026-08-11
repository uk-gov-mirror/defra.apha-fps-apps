using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.PactTimeCodeValidServiceTest
{
    public class PactTimeCodeValidServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactTimeCodeValidApiClient _pactTimeCodeValidApiClient;
        private readonly PactTimeCodeValidService _service;

        public PactTimeCodeValidServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactTimeCodeValidApiClient = Substitute.For<IPactTimeCodeValidApiClient>();
            _pactClient.PactTimeCodeValid.Returns(_pactTimeCodeValidApiClient);
            _service = new PactTimeCodeValidService(_pactClient);
        }

        #region GetPagedByProjectAndTestCodeAsync Tests

        [Fact]
        public async Task GetPagedByProjectAndTestCodeAsync_WithValidParams_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var timeCodes = new List<TimeCodeValidDto>
            {
                new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" }
            };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                timeCodes, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            _pactTimeCodeValidApiClient.GetPagedByProjectAndTestCodeAsync(query, "PP001", "TST001")
                .Returns(expectedResponse);

            var result = await _service.GetPagedByProjectAndTestCodeAsync(query, "PP001", "TST001");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactTimeCodeValidApiClient.Received(1).GetPagedByProjectAndTestCodeAsync(query, "PP001", "TST001");
        }

        [Fact]
        public async Task GetPagedByProjectAndTestCodeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.GetPagedByProjectAndTestCodeAsync(query, "PP001", "TST001")
                .Returns(expectedResponse);

            var result = await _service.GetPagedByProjectAndTestCodeAsync(query, "PP001", "TST001");

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetTimeCodeValidAsync Tests

        [Fact]
        public async Task GetTimeCodeValidAsync_WithValidKey_ReturnsSingleTimeCode()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            var expectedResponse = ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto);
            _pactTimeCodeValidApiClient.GetTimeCodeValidAsync("WG001", "TC001", "PP001")
                .Returns(expectedResponse);

            var result = await _service.GetTimeCodeValidAsync("WG001", "TC001", "PP001");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("TC001", result.Data?.TimeCode);
            await _pactTimeCodeValidApiClient.Received(1).GetTimeCodeValidAsync("WG001", "TC001", "PP001");
        }

        [Fact]
        public async Task GetTimeCodeValidAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<TimeCodeValidDto>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.GetTimeCodeValidAsync("WG_NONE", "TC_NONE", "PP001")
                .Returns(expectedResponse);

            var result = await _service.GetTimeCodeValidAsync("WG_NONE", "TC_NONE", "PP001");

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetByJobCodeAsync Tests

        [Fact]
        public async Task GetByJobCodeAsync_WithValidJobCodeAndProject_ReturnsTimeCodeList()
        {
            // Arrange
            var jobCode = "JC001";
            var parentProject = "PP001";
            var timeCodes = new List<TimeCodeValidDto>
            {
                new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject, JobCode = jobCode },
                new TimeCodeValidDto { TimeCode = "TC002", WorkGroup = "WG001", ParentProject = parentProject, JobCode = jobCode }
            };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(timeCodes);
            _pactTimeCodeValidApiClient.GetByJobCodeAsync(jobCode, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetByJobCodeAsync(jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactTimeCodeValidApiClient.Received(1).GetByJobCodeAsync(jobCode, parentProject);
        }

        [Fact]
        public async Task GetByJobCodeAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var jobCode = "JC001";
            var parentProject = "PP001";
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(new List<TimeCodeValidDto>());
            _pactTimeCodeValidApiClient.GetByJobCodeAsync(jobCode, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetByJobCodeAsync(jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetByJobCodeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var jobCode = "JC001";
            var parentProject = "PP001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.GetByJobCodeAsync(jobCode, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetByJobCodeAsync(jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetPagedTimeCodesAsync Tests

        [Fact]
        public async Task GetPagedTimeCodesAsync_WithValidQuery_ReturnsPaginatedTimeCodes()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JC001";
            var parentProject = "PP001";
            var timeCodes = new List<TimeCodeValidDto>
            {
                new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject }
            };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                timeCodes,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );
            _pactTimeCodeValidApiClient.GetPagedTimeCodesAsync(query, jobCode, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedTimeCodesAsync(query, jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactTimeCodeValidApiClient.Received(1).GetPagedTimeCodesAsync(query, jobCode, parentProject);
        }

        [Fact]
        public async Task GetPagedTimeCodesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.GetPagedTimeCodesAsync(query, null, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedTimeCodesAsync(query, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateTimeCodeValidAsync Tests

        [Fact]
        public async Task CreateTimeCodeValidAsync_WithValidItem_ReturnsSuccessResponse()
        {
            // Arrange
            var newItem = new TimeCodeValidDto
            {
                TimeCode = "TC001",
                WorkGroup = "WG001",
                ParentProject = "PP001",
                JobCode = "JC001",
                Active = true
            };
            var expectedResponse = ApiResponseDto<TimeCodeValidDto>.SuccessResponse(newItem);
            _pactTimeCodeValidApiClient.CreateTimeCodeValidAsync(newItem).Returns(expectedResponse);

            // Act
            var result = await _service.CreateTimeCodeValidAsync(newItem);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(newItem.TimeCode, result.Data?.TimeCode);
            await _pactTimeCodeValidApiClient.Received(1).CreateTimeCodeValidAsync(newItem);
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newItem = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate entry", Code = "DUPLICATE" } };
            var expectedResponse = ApiResponseDto<TimeCodeValidDto>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.CreateTimeCodeValidAsync(newItem).Returns(expectedResponse);

            // Act
            var result = await _service.CreateTimeCodeValidAsync(newItem);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateTimeCodeValidAsync Tests

        [Fact]
        public async Task UpdateTimeCodeValidAsync_WithValidItem_ReturnsSuccessResponse()
        {
            // Arrange
            var item = new TimeCodeValidDto
            {
                TimeCode = "TC001",
                WorkGroup = "WG001",
                ParentProject = "PP001",
                Active = false
            };
            var expectedResponse = ApiResponseDto<TimeCodeValidDto>.SuccessResponse(item);
            _pactTimeCodeValidApiClient.UpdateTimeCodeValidAsync(item).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateTimeCodeValidAsync(item);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(item.TimeCode, result.Data?.TimeCode);
            await _pactTimeCodeValidApiClient.Received(1).UpdateTimeCodeValidAsync(item);
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_WithNonExistentItem_ReturnsFailureResponse()
        {
            // Arrange
            var item = new TimeCodeValidDto { TimeCode = "NONEXISTENT", WorkGroup = "WG001", ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Time code not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<TimeCodeValidDto>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.UpdateTimeCodeValidAsync(item).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateTimeCodeValidAsync(item);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteTimeCodeValidAsync Tests

        [Fact]
        public async Task DeleteTimeCodeValidAsync_WithValidParams_ReturnsSuccessResponse()
        {
            // Arrange
            var workGroup = "WG001";
            var timeCode = "TC001";
            var parentProject = "PP001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactTimeCodeValidApiClient.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactTimeCodeValidApiClient.Received(1).DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);
        }

        [Fact]
        public async Task DeleteTimeCodeValidAsync_WithNonExistentEntry_ReturnsFailureResponse()
        {
            // Arrange
            var workGroup = "WG_NONE";
            var timeCode = "TC_NONE";
            var parentProject = "PP001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Entry not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetTimeCodeValidsByWorkGroupAsync Tests

        [Fact]
        public async Task GetTimeCodeValidsByWorkGroupAsync_WithValidWorkGroup_ReturnsSuccessResponse()
        {
            var expected = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                [new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" }]);
            _pactTimeCodeValidApiClient.GetTimeCodeValidsByWorkGroupAsync("WG001").Returns(expected);

            var result = await _service.GetTimeCodeValidsByWorkGroupAsync("WG001");

            Assert.Equal(expected, result);
            await _pactTimeCodeValidApiClient.Received(1).GetTimeCodeValidsByWorkGroupAsync("WG001");
        }

        [Fact]
        public async Task GetTimeCodeValidsByWorkGroupAsync_WithEmptyWorkGroup_ThrowsArgumentException()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetTimeCodeValidsByWorkGroupAsync(""));

            Assert.Equal("workGroup", ex.ParamName);
        }

        [Fact]
        public async Task GetTimeCodeValidsByWorkGroupAsync_WithNullWorkGroup_ThrowsArgumentException()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetTimeCodeValidsByWorkGroupAsync(null!));

            Assert.Equal("workGroup", ex.ParamName);
        }

        #endregion

        #region GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync Tests

        [Fact]
        public async Task GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync_WithValidInput_ReturnsSuccessResponse()
        {
            var expected = ApiResponseDto<List<string>>.SuccessResponse(["PP001", "PP002"]);
            _pactTimeCodeValidApiClient
                .GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG001", "TC001")
                .Returns(expected);

            var result = await _service.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG001", "TC001");

            Assert.Equal(expected, result);
            await _pactTimeCodeValidApiClient
                .Received(1)
                .GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG001", "TC001");
        }

        [Fact]
        public async Task GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync_WithEmptyWorkGroup_ThrowsArgumentException()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("", "TC001"));

            Assert.Equal("workGroup", ex.ParamName);
        }

        [Fact]
        public async Task GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync_WithEmptyTimeCode_ThrowsArgumentException()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG001", ""));

            Assert.Equal("timeCode", ex.ParamName);
        }

        #endregion

        #region GetAllDistinctTimeCodesAsync Tests

        [Fact]
        public async Task GetAllDistinctTimeCodesAsync_WhenApiReturnsSuccess_DelegatesAndReturnsResponse()
        {
            var expected = ApiResponseDto<List<string>>.SuccessResponse(["TC001", "TC002"]);
            _pactTimeCodeValidApiClient.GetAllDistinctTimeCodesAsync().Returns(expected);

            var result = await _service.GetAllDistinctTimeCodesAsync();

            Assert.Equal(expected, result);
            await _pactTimeCodeValidApiClient.Received(1).GetAllDistinctTimeCodesAsync();
        }

        #endregion

        #region GetAllDistinctProjectsAsync Tests

        [Fact]
        public async Task GetAllDistinctProjectsAsync_WhenApiReturnsSuccess_DelegatesAndReturnsResponse()
        {
            var expected = ApiResponseDto<List<string>>.SuccessResponse(["PP001", "PP002"]);
            _pactTimeCodeValidApiClient.GetAllDistinctProjectsAsync().Returns(expected);

            var result = await _service.GetAllDistinctProjectsAsync();

            Assert.Equal(expected, result);
            await _pactTimeCodeValidApiClient.Received(1).GetAllDistinctProjectsAsync();
        }

        #endregion

        #region DeleteAllByJobCodeAsync Tests

        [Fact]
        public async Task DeleteAllByJobCodeAsync_WithValidJobCode_ReturnsSuccessResponse()
        {
            // Arrange
            var jobCode = "JC001";
            var parentProject = "PP001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactTimeCodeValidApiClient.DeleteAllByJobCodeAsync(jobCode, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteAllByJobCodeAsync(jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactTimeCodeValidApiClient.Received(1).DeleteAllByJobCodeAsync(jobCode, parentProject);
        }

        [Fact]
        public async Task DeleteAllByJobCodeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var jobCode = "JC001";
            var parentProject = "PP001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.DeleteAllByJobCodeAsync(jobCode, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteAllByJobCodeAsync(jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CopyWorkGroupAsync Tests

        [Fact]
        public async Task CopyWorkGroupAsync_WithValidJobCodes_ReturnsCopiedTimeCodes()
        {
            // Arrange
            var sourceJobCode = "JC001";
            var targetJobCode = "JC002";
            var parentProject = "PP001";
            var copiedItems = new List<TimeCodeValidDto>
            {
                new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject, JobCode = targetJobCode },
                new TimeCodeValidDto { TimeCode = "TC002", WorkGroup = "WG001", ParentProject = parentProject, JobCode = targetJobCode }
            };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(copiedItems);
            _pactTimeCodeValidApiClient.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactTimeCodeValidApiClient.Received(1).CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);
        }

        [Fact]
        public async Task CopyWorkGroupAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var sourceJobCode = "JC001";
            var targetJobCode = "JC002";
            var parentProject = "PP001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Copy failed", Code = "COPY_ERROR" } };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteBulkAsync Tests

        [Fact]
        public async Task DeleteBulkAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new BulkDeleteTimeCodeRequestDto
            {
                ParentProject = "PP001",
                Items = [new TimeCodeKeyItemDto { WorkGroup = "WG001", TimeCode = "TC001" }]
            };
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactTimeCodeValidApiClient.DeleteBulkAsync(request).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteBulkAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactTimeCodeValidApiClient.Received(1).DeleteBulkAsync(request);
        }

        [Fact]
        public async Task DeleteBulkAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var request = new BulkDeleteTimeCodeRequestDto
            {
                ParentProject = "PP001",
                Items = [new TimeCodeKeyItemDto { WorkGroup = "WG001", TimeCode = "TC001" }]
            };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Bulk delete failed", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.DeleteBulkAsync(request).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteBulkAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task DeleteBulkAsync_WithEmptyItems_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new BulkDeleteTimeCodeRequestDto { ParentProject = "PP001", Items = [] };
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactTimeCodeValidApiClient.DeleteBulkAsync(request).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteBulkAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        #endregion

        #region CopySelectedWorkGroupsAsync Tests

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithValidRequest_ReturnsCopiedItems()
        {
            // Arrange
            var request = new BulkCopyWorkGroupRequestDto
            {
                ParentProject = "PP001",
                SourceJobCode = "JC001",
                TargetJobCode = "JC002",
                WorkGroups = ["WG001", "WG002"]
            };
            var copiedItems = new List<TimeCodeValidDto>
            {
                new TimeCodeValidDto { TimeCode = "JC002", WorkGroup = "WG001", ParentProject = "PP001", JobCode = "JC002" },
                new TimeCodeValidDto { TimeCode = "JC002", WorkGroup = "WG002", ParentProject = "PP001", JobCode = "JC002" }
            };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(copiedItems);
            _pactTimeCodeValidApiClient.CopySelectedWorkGroupsAsync(request).Returns(expectedResponse);

            // Act
            var result = await _service.CopySelectedWorkGroupsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactTimeCodeValidApiClient.Received(1).CopySelectedWorkGroupsAsync(request);
        }

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var request = new BulkCopyWorkGroupRequestDto
            {
                ParentProject = "PP001",
                SourceJobCode = "JC001",
                TargetJobCode = "JC002",
                WorkGroups = ["WG001"]
            };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Copy failed", Code = "COPY_ERROR" } };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactTimeCodeValidApiClient.CopySelectedWorkGroupsAsync(request).Returns(expectedResponse);

            // Act
            var result = await _service.CopySelectedWorkGroupsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithEmptyWorkGroups_ReturnsEmptyList()
        {
            // Arrange
            var request = new BulkCopyWorkGroupRequestDto
            {
                ParentProject = "PP001",
                SourceJobCode = "JC001",
                TargetJobCode = "JC002",
                WorkGroups = []
            };
            var expectedResponse = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([]);
            _pactTimeCodeValidApiClient.CopySelectedWorkGroupsAsync(request).Returns(expectedResponse);

            // Act
            var result = await _service.CopySelectedWorkGroupsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion
    }
}