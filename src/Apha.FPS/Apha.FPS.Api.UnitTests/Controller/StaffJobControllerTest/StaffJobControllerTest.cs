using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.StaffJobControllerTest
{
    public class StaffJobControllerTest
    {
        private readonly IStaffJobService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly StaffJobController _controller;

        public StaffJobControllerTest()
        {
            _serviceMock = Substitute.For<IStaffJobService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new StaffJobController(_serviceMock, _mapperMock);
        }

        #region GetJobStaffCostAsync

        [Fact]
        public async Task GetJobStaffCostAsync_HappyPath_ReturnsOk()
        {
            var query = new PaginationReq<string>();
            var serviceResult = new PaginatedResult<StaffJobViewDto>();
            var mappedResult = new PaginationRes<StaffJobViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetJobStaffCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<StaffJobViewRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetJobStaffCostAsync(query, "");

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_EdgeCase_EmptyResult()
        {
            var query = new PaginationReq<string>();
            var serviceResult = new PaginatedResult<StaffJobViewDto>();
            var mappedResult = new PaginationRes<StaffJobViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetJobStaffCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<StaffJobViewRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetJobStaffCostAsync(query, "");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Error_ServiceThrows()
        {
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetJobStaffCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>()).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetJobStaffCostAsync(query, ""));
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Error_MapperThrows()
        {
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetJobStaffCostAsync(query, ""));
        }

        #endregion

        #region GetStaffWorkgroupLookup

        [Fact]
        public async Task GetStaffWorkgroupLookup_HappyPath_ReturnsOk()
        {
            var serviceResult = new List<StaffWorkgroupLookupDto>();
            var mappedResult = new List<StaffWorkgroupLookupRes>();

            _serviceMock.GetStaffWorkgroupLookup().Returns(serviceResult);
            _mapperMock.Map<List<StaffWorkgroupLookupRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetStaffWorkgroupLookup();

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_EdgeCase_EmptyList()
        {
            var serviceResult = new List<StaffWorkgroupLookupDto>();
            var mappedResult = new List<StaffWorkgroupLookupRes>();

            _serviceMock.GetStaffWorkgroupLookup().Returns(serviceResult);
            _mapperMock.Map<List<StaffWorkgroupLookupRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetStaffWorkgroupLookup();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_Error_ServiceThrows()
        {
            _serviceMock.GetStaffWorkgroupLookup().Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffWorkgroupLookup());
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_Error_MapperThrows()
        {
            var serviceResult = new List<StaffWorkgroupLookupDto>();
            _serviceMock.GetStaffWorkgroupLookup().Returns(serviceResult);
            _mapperMock.Map<List<StaffWorkgroupLookupRes>>(serviceResult).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffWorkgroupLookup());
        }

        #endregion

        #region GetStaffChargeRate

        [Fact]
        public async Task GetStaffChargeRate_HappyPath_ReturnsOk()
        {
            _serviceMock.GetStaffChargeRate("S1", "J1").Returns(100m);

            var result = await _controller.GetStaffChargeRate("S1", "J1");

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(100m, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetStaffChargeRate_EdgeCase_NullResult()
        {
            _serviceMock.GetStaffChargeRate("S1", "J1").Returns((decimal?)null);

            var result = await _controller.GetStaffChargeRate("S1", "J1");

            Assert.IsType<OkObjectResult>(result);
            Assert.Null(((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetStaffChargeRate_Error_ServiceThrows()
        {
            _serviceMock.GetStaffChargeRate("S1", "J1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffChargeRate("S1", "J1"));
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HappyPath_ReturnsOk()
        {
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            var mapped = new StaffJobRes();

            _serviceMock.GetByIdAsync("S1", "J1").Returns(dto);
            _mapperMock.Map<StaffJobRes>(dto).Returns(mapped);

            var result = await _controller.GetByIdAsync("S1", "J1");

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetByIdAsync_EdgeCase_NullResult_ThrowsKeyNotFound()
        {
            _serviceMock.GetByIdAsync("S1", "J1").Returns((StaffJobDto)null!);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetByIdAsync("S1", "J1"));
        }

        [Fact]
        public async Task GetByIdAsync_Error_ServiceThrows()
        {
            _serviceMock.GetByIdAsync("S1", "J1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetByIdAsync("S1", "J1"));
        }

        [Fact]
        public async Task GetByIdAsync_Error_MapperThrows()
        {
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            _serviceMock.GetByIdAsync("S1", "J1").Returns(dto);
            _mapperMock.Map<StaffJobRes>(dto).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetByIdAsync("S1", "J1"));
        }

        #endregion

        #region GetViewByStaffIdAsync

        [Fact]
        public async Task GetViewByStaffIdAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var serviceResult = new StaffJobViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "John Doe",
                PlannedHours = 40,
                ChargeRate = 150.00m
            };

            var mappedResult = new StaffJobViewRes
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "John Doe",
                PlannedHours = 40,
                ChargeRate = 150.00m
            };

            _serviceMock.GetViewByStaffIdAsync(staffId, jobCode).Returns(serviceResult);
            _mapperMock.Map<StaffJobViewRes>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(mappedResult, okResult.Value);

            await _serviceMock.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mapperMock.Received(1).Map<StaffJobViewRes>(serviceResult);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_EdgeCase_NullResult_ReturnsOkWithNull()
        {
            // Arrange
            var staffId = "STAFF999";
            var jobCode = "JOB999";

            _serviceMock.GetViewByStaffIdAsync(staffId, jobCode).Returns((StaffJobViewDto?)null);
            _mapperMock.Map<StaffJobViewRes>(null).Returns((StaffJobViewRes?)null);

            // Act
            var result = await _controller.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Null(okResult.Value);

            await _serviceMock.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mapperMock.Received(1).Map<StaffJobViewRes>(null);
        }

        [Theory]
        [InlineData("", "JOB001")]
        [InlineData("STAFF001", "")]
        [InlineData("", "")]
        public async Task GetViewByStaffIdAsync_EdgeCase_EmptyParameters_CallsService(string staffId, string jobCode)
        {
            // Arrange
            var serviceResult = new StaffJobViewDto();
            var mappedResult = new StaffJobViewRes();

            _serviceMock.GetViewByStaffIdAsync(staffId, jobCode).Returns(serviceResult);
            _mapperMock.Map<StaffJobViewRes>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_EdgeCase_EmptyObject_ReturnsOkWithEmptyObject()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var serviceResult = new StaffJobViewDto { StaffID = staffId, JobCode = jobCode };
            var emptyMappedResult = new StaffJobViewRes();

            _serviceMock.GetViewByStaffIdAsync(staffId, jobCode).Returns(serviceResult);
            _mapperMock.Map<StaffJobViewRes>(serviceResult).Returns(emptyMappedResult);

            // Act
            var result = await _controller.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            var resultObj = okResult.Value as StaffJobViewRes;
            Assert.NotNull(resultObj);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WithCompleteData_ReturnsMappedObject()
        {
            // Arrange
            var staffId = "STAFF002";
            var jobCode = "JOB002";

            var serviceResult = new StaffJobViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "Jane Smith",
                PlannedHours = 80,
                ChargeRate = 200.00m,
                StaffCost = 16000.00m,
                WorkGroupGrade = "WG01",
                GradeCode = "G01",
                WorkGroup = "Engineering",
                SectorName = "charge",
                Days = 10
            };

            var mappedResult = new StaffJobViewRes
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "Jane Smith",
                PlannedHours = 80,
                ChargeRate = 200.00m,
                StaffCost = 16000.00m,
                WorkGroupGrade = "WG01",
                GradeCode = "G01",
                WorkGroup = "Engineering",
                SectorName = "charge",
                Days = 10
            };

            _serviceMock.GetViewByStaffIdAsync(staffId, jobCode).Returns(serviceResult);
            _mapperMock.Map<StaffJobViewRes>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            var resultObj = okResult.Value as StaffJobViewRes;
            Assert.NotNull(resultObj);
            Assert.Equal(staffId, resultObj.StaffID);
            Assert.Equal(jobCode, resultObj.JobCode);
            Assert.Equal("Jane Smith", resultObj.Name);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_Error_ServiceThrows()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            _serviceMock.GetViewByStaffIdAsync(staffId, jobCode)
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _controller.GetViewByStaffIdAsync(staffId, jobCode));

            await _serviceMock.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mapperMock.DidNotReceive().Map<StaffJobViewRes>(Arg.Any<StaffJobViewDto>());
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_Error_MapperThrows()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var serviceResult = new StaffJobViewDto { StaffID = staffId, JobCode = jobCode };

            _serviceMock.GetViewByStaffIdAsync(staffId, jobCode).Returns(serviceResult);
            _mapperMock.Map<StaffJobViewRes>(serviceResult)
                .Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _controller.GetViewByStaffIdAsync(staffId, jobCode));

            await _serviceMock.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_Error_InvalidOperationException()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            _serviceMock.GetViewByStaffIdAsync(staffId, jobCode)
                .Throws(new InvalidOperationException("Invalid operation"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _controller.GetViewByStaffIdAsync(staffId, jobCode));

            Assert.Equal("Invalid operation", exception.Message);
            await _serviceMock.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_HappyPath_ReturnsCreated()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            var resultDto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            var mapped = new StaffJobRes();

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.AddAsync(dto).Returns(resultDto);
            _mapperMock.Map<StaffJobRes>(resultDto).Returns(mapped);

            var result = await _controller.AddAsync(req);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(mapped, createdResult.Value);
        }

        [Fact]
        public async Task AddAsync_EdgeCase_MinimalInput()
        {
            var req = new StaffJobReq { StaffId = "", JobCode = "" };
            var dto = new StaffJobDto { StaffId = "", JobCode = "" };
            var resultDto = new StaffJobDto { StaffId = "", JobCode = "" };
            var mapped = new StaffJobRes();

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.AddAsync(dto).Returns(resultDto);
            _mapperMock.Map<StaffJobRes>(resultDto).Returns(mapped);

            var result = await _controller.AddAsync(req);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task AddAsync_Error_ServiceThrows()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.AddAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.AddAsync(req));
        }

        [Fact]
        public async Task AddAsync_Error_MapperThrows()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            _mapperMock.Map<StaffJobDto>(req).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.AddAsync(req));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HappyPath_ReturnsOk()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            var resultDto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };
            var mapped = new StaffJobRes();

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(resultDto);
            _mapperMock.Map<StaffJobRes>(resultDto).Returns(mapped);

            var result = await _controller.UpdateAsync(req);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task UpdateAsync_EdgeCase_MinimalInput()
        {
            var req = new StaffJobReq { StaffId = "", JobCode = "" };
            var dto = new StaffJobDto { StaffId = "", JobCode = "" };
            var resultDto = new StaffJobDto { StaffId = "", JobCode = "" };
            var mapped = new StaffJobRes();

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(resultDto);
            _mapperMock.Map<StaffJobRes>(resultDto).Returns(mapped);

            var result = await _controller.UpdateAsync(req);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAsync_Error_ServiceThrows()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            var dto = new StaffJobDto { StaffId = "S1", JobCode = "J1" };

            _mapperMock.Map<StaffJobDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateAsync(req));
        }

        [Fact]
        public async Task UpdateAsync_Error_MapperThrows()
        {
            var req = new StaffJobReq { StaffId = "S1", JobCode = "J1" };
            _mapperMock.Map<StaffJobDto>(req).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateAsync(req));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HappyPath_ReturnsNoContent()
        {
            _serviceMock.DeleteAsync("S1", "J1").Returns(true);

            var result = await _controller.DeleteAsync("S1", "J1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }
        

        [Fact]
        public async Task DeleteAsync_Error_ServiceThrows()
        {
            _serviceMock.DeleteAsync("S1", "J1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteAsync("S1", "J1"));
        }     

        #endregion

        #region GetTotalStaffCostAsync

        [Fact]
        public async Task GetTotalStaffCostAsync_HappyPath_ReturnsOkWithTotal()
        {
            var jobCode = "JOB001";
            var total = 4500m;
            _serviceMock.GetTotalStaffCostAsync(jobCode).Returns(total);

            var result = await _controller.GetTotalStaffCostAsync(jobCode);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(total, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetTotalStaffCostAsync_ReturnsZero_WhenNoStaffCost()
        {
            var jobCode = "JOB001";
            _serviceMock.GetTotalStaffCostAsync(jobCode).Returns(0m);

            var result = await _controller.GetTotalStaffCostAsync(jobCode);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0m, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetTotalStaffCostAsync_Error_ServiceThrows()
        {
            _serviceMock.GetTotalStaffCostAsync("JOB001").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetTotalStaffCostAsync("JOB001"));
        }

        [Theory]
        [InlineData("JOB001")]
        [InlineData("FZ2000")]
        [InlineData("PROJ123")]
        public async Task GetTotalStaffCostAsync_WithVariousJobCodes_CallsService(string jobCode)
        {
            _serviceMock.GetTotalStaffCostAsync(jobCode).Returns(100m);

            var result = await _controller.GetTotalStaffCostAsync(jobCode);

            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        #endregion

        #region GetZtStaffJobsByStaffIdPagedAsync

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_HappyPath_ReturnsOk()
        {
            var query = new PaginationReq<string>();
            var serviceResult = new PaginatedResult<StaffJobZtViewDto>();
            var mappedResult = new PaginationRes<StaffJobZtViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetZtStaffJobsByStaffIdPagedAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<StaffJobZtViewRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_EdgeCase_EmptyResult()
        {
            var query = new PaginationReq<string>();
            var serviceResult = new PaginatedResult<StaffJobZtViewDto>();
            var mappedResult = new PaginationRes<StaffJobZtViewRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetZtStaffJobsByStaffIdPagedAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<StaffJobZtViewRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_Error_ServiceThrows()
        {
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetZtStaffJobsByStaffIdPagedAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>()).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetZtStaffJobsByStaffIdPagedAsync(query, "S001"));
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_Error_MapperThrows()
        {
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Throws(new Exception("Mapping error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetZtStaffJobsByStaffIdPagedAsync(query, "S001"));
        }

        #endregion

        #region GetZtStaffJobDetailsByIdAsync

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_HappyPath_ReturnsOk()
        {
            var dto = new StaffJobZtViewDto { StaffID = "S1", JobCode = "ZT1", PlannedHours = 40 };
            var mapped = new StaffJobZtViewRes { StaffID = "S1", JobCode = "ZT1", PlannedHours = 40 };

            _serviceMock.GetZtStaffJobDetailsByIdAsync("S1", "ZT1").Returns(dto);
            _mapperMock.Map<StaffJobZtViewRes>(dto).Returns(mapped);

            var result = await _controller.GetZtStaffJobDetailsByIdAsync("S1", "ZT1");

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_EdgeCase_NullResult_ThrowsKeyNotFound()
        {
            _serviceMock.GetZtStaffJobDetailsByIdAsync("S1", "ZT1").Returns((StaffJobZtViewDto)null!);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetZtStaffJobDetailsByIdAsync("S1", "ZT1"));
        }

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_Error_ServiceThrows()
        {
            _serviceMock.GetZtStaffJobDetailsByIdAsync("S1", "ZT1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetZtStaffJobDetailsByIdAsync("S1", "ZT1"));
        }

        [Theory]
        [InlineData("S001", "ZT001")]
        [InlineData("S002", "ZT002")]
        [InlineData("EMP123", "ZT_TEST")]
        public async Task GetZtStaffJobDetailsByIdAsync_WithVariousIds_CallsService(string staffId, string jobCode)
        {
            var dto = new StaffJobZtViewDto { StaffID = staffId, JobCode = jobCode };
            var mapped = new StaffJobZtViewRes();

            _serviceMock.GetZtStaffJobDetailsByIdAsync(staffId, jobCode).Returns(dto);
            _mapperMock.Map<StaffJobZtViewRes>(dto).Returns(mapped);

            await _controller.GetZtStaffJobDetailsByIdAsync(staffId, jobCode);

            await _serviceMock.Received(1).GetZtStaffJobDetailsByIdAsync(staffId, jobCode);
        }

        #endregion

        #region GetZtTotalHoursByStaffIdAsync

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_HappyPath_ReturnsOk()
        {
            _serviceMock.GetZtTotalHoursByStaffIdAsync("S1").Returns(120.5);

            var result = await _controller.GetZtTotalHoursByStaffIdAsync("S1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(120.5, okResult.Value);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_ZeroHours_ReturnsOkWithZero()
        {
            _serviceMock.GetZtTotalHoursByStaffIdAsync("S1").Returns(0.0);

            var result = await _controller.GetZtTotalHoursByStaffIdAsync("S1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0.0, okResult.Value);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetZtTotalHoursByStaffIdAsync("S1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetZtTotalHoursByStaffIdAsync("S1"));
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_CallsServiceOnce()
        {
            _serviceMock.GetZtTotalHoursByStaffIdAsync("S1").Returns(50.0);

            await _controller.GetZtTotalHoursByStaffIdAsync("S1");

            await _serviceMock.Received(1).GetZtTotalHoursByStaffIdAsync("S1");
        }

        #endregion

        #region GetStaffSummaryByIdAsync

        [Fact]
        public async Task GetStaffSummaryByIdAsync_HappyPath_ReturnsOk()
        {
            var dto = new StaffWorkgroupLookupDto { StaffID = "S1", Name = "John", WorkGroupGrade = "A1" };
            var mapped = new StaffWorkgroupLookupRes { StaffID = "S1", Name = "John", WorkGroupGrade = "A1" };

            _serviceMock.GetStaffSummaryByIdAsync("S1").Returns(dto);
            _mapperMock.Map<StaffWorkgroupLookupRes>(dto).Returns(mapped);

            var result = await _controller.GetStaffSummaryByIdAsync("S1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_NotFound_ReturnsNotFound()
        {
            _serviceMock.GetStaffSummaryByIdAsync("S1").Returns((StaffWorkgroupLookupDto?)null);

            var result = await _controller.GetStaffSummaryByIdAsync("S1");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetStaffSummaryByIdAsync("S1").Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffSummaryByIdAsync("S1"));
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_CallsServiceOnce()
        {
            _serviceMock.GetStaffSummaryByIdAsync("S1").Returns(new StaffWorkgroupLookupDto());
            _mapperMock.Map<StaffWorkgroupLookupRes>(Arg.Any<StaffWorkgroupLookupDto>()).Returns(new StaffWorkgroupLookupRes());

            await _controller.GetStaffSummaryByIdAsync("S1");

            await _serviceMock.Received(1).GetStaffSummaryByIdAsync("S1");
        }

        #endregion

        #region GetStaffResourceUtilisationAsync

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string>();
            const string workgroup = "WG01";
            var queryParams = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<StaffResourceUtilisationDto>
            {
                Data = new List<StaffResourceUtilisationDto>
                {
                    new() { WorkGroup = workgroup, Name = "John Doe", WgGrade = "GR1", HrsAvail = 37.5 }
                },
                PaginationData = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };
            var mappedRes = new PaginationRes<StaffResourceUtilisationRes>
            {
                Data = new List<StaffResourceUtilisationRes>
                {
                    new() { WorkGroup = workgroup, Name = "John Doe", WgGrade = "GR1", HrsAvail = 37.5 }
                }
            };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(queryParams);
            _serviceMock.GetStaffResourceUtilisationAsync(queryParams, workgroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<StaffResourceUtilisationRes>>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedRes, okResult.Value);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new PaginationReq<string>();
            const string workgroup = "WG_NONE";
            var queryParams = new QueryParameters<string>();
            var emptyServiceResult = new PaginatedResult<StaffResourceUtilisationDto>
            {
                Data = new List<StaffResourceUtilisationDto>(),
                PaginationData = new PaginationDto { TotalRecords = 0, PageNumber = 1, PageSize = 10 }
            };
            var emptyRes = new PaginationRes<StaffResourceUtilisationRes> { Data = new List<StaffResourceUtilisationRes>() };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(queryParams);
            _serviceMock.GetStaffResourceUtilisationAsync(queryParams, workgroup).Returns(emptyServiceResult);
            _mapperMock.Map<PaginationRes<StaffResourceUtilisationRes>>(emptyServiceResult).Returns(emptyRes);

            // Act
            var result = await _controller.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyRes, okResult.Value);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_CallsServiceOnce()
        {
            // Arrange
            var query = new PaginationReq<string>();
            const string workgroup = "WG01";
            var queryParams = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<StaffResourceUtilisationDto>();
            var mappedRes = new PaginationRes<StaffResourceUtilisationRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(queryParams);
            _serviceMock.GetStaffResourceUtilisationAsync(queryParams, workgroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<StaffResourceUtilisationRes>>(serviceResult).Returns(mappedRes);

            // Act
            await _controller.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            await _serviceMock.Received(1).GetStaffResourceUtilisationAsync(queryParams, workgroup);
        }

        #endregion
    }
}
