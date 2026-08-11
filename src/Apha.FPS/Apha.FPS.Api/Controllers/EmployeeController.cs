using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// Controller for managing employee-related operations.
    /// </summary>    
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeController"/> class.
        /// </summary>
        /// <param name="employeeService">The employee service.</param>
        /// <param name="mapper">The AutoMapper instance.</param>
        public EmployeeController(
                        IEmployeeService employeeService,
                        IMapper mapper)
        {
            _employeeService = employeeService;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets a paginated and filtered list of employees based on filter option.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="filterOption">The filter option to apply (1=All, 2=Prefix 'T', 3=Prefix 'G').</param>
        /// <returns>A paginated and filtered list of employees.</returns>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetFilteredEmployeesAsync([FromQuery] PaginationReq<string> query, [FromQuery] int filterOption)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _employeeService.GetFilteredEmployeesAsync(filter, filterOption);
            return Ok(_mapper.Map<PaginationRes<EmployeeRes>>(result));
        }

        /// <summary>
        /// Gets filtered employees based on filter option.
        /// </summary>
        /// <param name="filterOption">The filter option to apply.</param>
        /// <returns>A filtered list of employees.</returns>
        [HttpGet("filtered")]
        public async Task<IActionResult> GetFilteredEmployeesAsync([FromQuery] int filterOption)
        {
            var result = await _employeeService.GetFilteredEmployeesAsync(filterOption);
            return Ok(_mapper.Map<List<EmployeeRes>>(result));
        }

        /// <summary>
        /// Gets an employee by their SP number.
        /// </summary>
        /// <param name="spNumber">The SP number of the employee.</param>
        /// <returns>The employee details, or NotFound if not found.</returns>
        [HttpGet("{spNumber}")]
        public async Task<IActionResult> GetEmployeeByIdAsync(string spNumber)
        {
            var result = await _employeeService.GetEmployeeByIdAsync(spNumber);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<EmployeeRes>(result));
        }

        /// <summary>
        /// Adds a new employee.
        /// </summary>
        /// <param name="employeeReq">The employee data to add.</param>
        /// <returns>The created employee.</returns>
        [HttpPost]
        public async Task<IActionResult> AddEmployeeAsync(EmployeeReq employeeReq)
        {
            var mapEmployeeReq = _mapper.Map<EmployeeDto>(employeeReq);
            var result = await _employeeService.AddEmployeeAsync(mapEmployeeReq);
            return Ok(_mapper.Map<EmployeeRes>(result));
        }

        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        /// <param name="employeeReq">The employee data to update.</param>
        /// <returns>The updated employee.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateEmployeeAsync(EmployeeReq employeeReq)
        {
            var mapEmployeeReq = _mapper.Map<EmployeeDto>(employeeReq);
            var result = await _employeeService.UpdateEmployeeAsync(mapEmployeeReq);
            return Ok(_mapper.Map<EmployeeRes>(result));
        }

        /// <summary>
        /// Deletes an employee by their SP number.
        /// </summary>
        /// <param name="spNumber">The SP number of the employee to delete.</param>
        /// <returns>True if deleted; otherwise, throws if not found.</returns>
        [HttpDelete("{spNumber}")]
        public async Task<IActionResult> DeleteEmployeeAsync(string spNumber)
        {
            var isDeleted = await _employeeService.DeleteEmployeeAsync(spNumber);
            if (!isDeleted)
            {
                throw new KeyNotFoundException("Employee not found.");
            }
            return Ok(isDeleted);
        }

        /// <summary>
        /// Gets a lookup list of all managers.
        /// </summary>
        /// <returns>A list of managers.</returns>
        [HttpGet("managers")]
        public async Task<IActionResult> GetAllManagersAsync()
        {
            var result = await _employeeService.GetAllManagersAsync();
            return Ok(_mapper.Map<List<ManagerRes>>(result));
        }

        /// <summary>
        /// Gets a lookup list of all managers for pact
        /// </summary>
        /// <returns></returns>
        [HttpGet("pactmanagers")]
        public async Task<IActionResult> GetAllPactManagersAsync()
        {
            var result = await _employeeService.GetAllPactManagersAsync();
            return Ok(_mapper.Map<List<ManagerRes>>(result));
        }

        /// <summary>
        /// <summary>
        /// Gets all persons (PACT staff joined with work group).
        /// </summary>
        /// <returns>A list of persons with Name, WorkGroupGrade and WorkGroup.</returns>
        [HttpGet("persons")]
        public async Task<IActionResult> GetAllWorkGroupPersonAsync()
        {
            var result = await _employeeService.GetAllWorkGroupPersonAsync();
            return Ok(_mapper.Map<List<WorkGroupPersonRes>>(result));
        }

        /// <summary>
        /// Gets a paginated, filtered and sorted list of all PACT staff people, optionally filtered by work group.
        /// </summary>
        [HttpGet("WorkGroupStaff/paginated")]
        public async Task<IActionResult> GetWorkGroupStaffPaginatedAsync([FromQuery] PaginationReq<string> query, [FromQuery] string? workGroup = null)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _employeeService.GetPagedWorkGroupStaffAsync(filter, workGroup);
            return Ok(_mapper.Map<PaginationRes<PactStaffRes>>(result));
        }

        /// <summary>
        /// Gets an unpaged list of all PACT staff for dropdown population.
        /// </summary>
        [HttpGet("PactStaff")]
        public async Task<IActionResult> GetAllPactStaffAsync()
        {
            var result = await _employeeService.GetPactStaffAsync();
            return Ok(_mapper.Map<List<PactStaffRes>>(result));
        }

        /// <summary>
        /// Gets a list of PACT staff for a specific work group.
        /// </summary>
        /// <param name="workGroup">The work group to filter by.</param>
        /// <returns>A list of PACT staff for the specified work group.</returns>
        [HttpGet("PactWorkGroupStaff")]
        public async Task<IActionResult> GetPactWorkGroupStaffAsync([FromQuery] string? workGroup)
        {
            var result = await _employeeService.GetPactWorkGroupStaffAsync(workGroup);
            return Ok(_mapper.Map<List<PactStaffRes>>(result));
        }
    }
}
