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
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin,API-FPSShared")]
    [Route("api/v{version:apiVersion}/wgstaff")]
    [ApiController]
    [ApiVersion("1.0")]
    public class WorkGroupEmployeeController : ControllerBase
    {
        private readonly IWorkGroupEmployeeService _workGroupEmployeeService;
        private readonly IMapper _mapper;

        public WorkGroupEmployeeController(
            IWorkGroupEmployeeService workGroupEmployeeService,
            IMapper mapper)
        {
            _workGroupEmployeeService = workGroupEmployeeService ?? throw new ArgumentNullException(nameof(workGroupEmployeeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult> GetWorkGroupEmployeeAsync(
            [FromQuery] PaginationReq<string> query,
            [FromQuery] string? wgGrade)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _workGroupEmployeeService.GetWorkGroupEmployeeAsync(filter, wgGrade ?? string.Empty);
            return Ok(_mapper.Map<PaginationRes<WorkGroupEmployeeRes>>(result));
        }

        [HttpGet("staff")]
        public async Task<ActionResult> GetWorkGroupEmployeeForStaffAsync(
            [FromQuery] PaginationReq<string> query,
            [FromQuery] string? wgGrade)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _workGroupEmployeeService.GetWorkGroupEmployeeForStaffAsync(filter, wgGrade ?? string.Empty);
            return Ok(_mapper.Map<PaginationRes<WorkGroupEmployeeRes>>(result));
        }

        [HttpGet("activestaff")]
        public async Task<ActionResult> GetAllActiveWorkGroupEmployeesAsync(
            [FromQuery] PaginationReq<string> query,
            [FromQuery] string? wgGrade)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _workGroupEmployeeService.GetAllActiveWorkGroupEmployeesAsync(filter, wgGrade ?? string.Empty);
            return Ok(_mapper.Map<PaginationRes<WorkGroupEmployeeRes>>(result));
        }

        [HttpGet("{pactId}")]
        public async Task<ActionResult<WorkGroupEmployeeRes>> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            var result = await _workGroupEmployeeService.GetWorkGroupEmployeeByIdAsync(pactId);
            if (result == null)
            {
                throw new KeyNotFoundException($"WorkGroupEmployee with PACT Id '{pactId}' not found.");
            }

            return Ok(_mapper.Map<WorkGroupEmployeeRes>(result));
        }

        [HttpPost("staff")]
        public async Task<ActionResult<WorkGroupEmployeeRes>> CreateWorkGroupEmployeeForStaffAsync([FromBody] WorkGroupEmployeeReq req)
        {
            var mappedDto = _mapper.Map<WorkGroupEmployeeDto>(req);
            var createdDto = await _workGroupEmployeeService.CreateWorkGroupEmployeeForStaffAsync(mappedDto);
            return Ok(_mapper.Map<WorkGroupEmployeeRes>(createdDto));
        }

        [HttpPut]
        public async Task<ActionResult<WorkGroupEmployeeRes>> UpdateWorkGroupEmployeeAsync([FromBody] WorkGroupEmployeeReq req)
        {
            var mappedDto = _mapper.Map<WorkGroupEmployeeDto>(req);
            var updatedDto = await _workGroupEmployeeService.UpdateWorkGroupEmployeeAsync(mappedDto);
            return Ok(_mapper.Map<WorkGroupEmployeeRes>(updatedDto));
        }

        [HttpPut("staff")]
        public async Task<ActionResult<WorkGroupEmployeeRes>> UpdateWorkGroupEmployeeForStaffAsync([FromBody] WorkGroupEmployeeReq req)
        {
            var mappedDto = _mapper.Map<WorkGroupEmployeeDto>(req);
            var updatedDto = await _workGroupEmployeeService.UpdateWorkGroupEmployeeForStaffAsync(mappedDto);
            return Ok(_mapper.Map<WorkGroupEmployeeRes>(updatedDto));
        }

        [HttpDelete("{pactId}")]
        public async Task<IActionResult> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            if (string.IsNullOrWhiteSpace(pactId))
                throw new ArgumentException("PACT Id cannot be null or empty.", nameof(pactId));

            var isDeleted = await _workGroupEmployeeService.DeleteWorkGroupEmployeeAsync(pactId);
            if (!isDeleted)
            {
                throw new KeyNotFoundException($"WorkGroupEmployee with PACT Id '{pactId}' not found.");
            }

            return Ok(isDeleted);
        }
    }
}
