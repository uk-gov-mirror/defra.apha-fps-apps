using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/program")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ProgramController : ControllerBase
    {
        private readonly IProgramService _programService;       
        private readonly IMapper _mapper;       
        public ProgramController(
            IProgramService programService,            
            IMapper mapper
            )
        {
            _programService = programService ?? throw new ArgumentNullException(nameof(programService));           
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));            
        }


        [HttpGet]
        public async Task<ActionResult> GetAllProgramsAsync()
        {
            var programDto = await _programService.GetAllProgramsAsync();
            if (programDto == null)
            {
                throw new ArgumentException("Program records not found for deletion");
            }
            return Ok(_mapper.Map<List<ProgramRes>>(programDto));
        }

        [HttpGet("all")]
        public async Task<ActionResult> GetAllProgramsForAllUsersAsync()
        {
            var programDto = await _programService.GetAllProgramsForAllUsersAsync();
            if (programDto == null)
            {
                throw new ArgumentException("Program records not found");
            }
            return Ok(_mapper.Map<List<ProgramRes>>(programDto));
        }

        [HttpGet("paged")]
        public async Task<ActionResult> GetAllProgramsPagedAsync(
            [FromQuery] QueryParameters<string> query)
        {
            var programDto = await _programService.GetAllProgramsAsync(query);
            if (programDto == null)
            {
                throw new ArgumentException("Program records not found for deletion");
            }
            return Ok(_mapper.Map<PaginationRes<ProgramRes>>(programDto));
        }   

        [HttpGet("time-snapshot/paged")]
        public async Task<ActionResult> GetProgramTimeSnapshotAsync(
            [FromQuery] QueryParameters<string> query)
        {
            var planCostDto = await _programService.GetProgramTimeSnapshotAsync(query);
            if (planCostDto == null)
            {
                throw new ArgumentException("Program time snapshot records not found");
            }
            return Ok(_mapper.Map<PaginationRes<ProgramPlanCostRes>>(planCostDto));
        }

        [HttpGet("{programNo}")]
        public async Task<ActionResult<ProgramRes>> GetProgramById(
            string programNo)
        {
            var programDto = await _programService.GetProgramByIdAsync(programNo);
            if (programDto == null)
            {
                throw new ArgumentException("Program record with ID: {ProgramId} not found for deletion", programNo);
            }            
            return Ok(_mapper.Map<ProgramRes>(programDto));
        }        

        [HttpPost]
        public async Task<ActionResult<ProgramRes>> CreateProgram(
            [FromBody] ProgramReq programViewModel)
        {
            var mappedProgramDto = _mapper.Map<ProgramDto>(programViewModel);
            var addProgramDto = await _programService.AddProgramAsync(mappedProgramDto);
            return Ok(_mapper.Map<ProgramRes>(addProgramDto));
        }

        [HttpPut]
        public async Task<ActionResult<ProgramRes>> UpdateProgram(
            [FromBody] ProgramReq programViewModel)
        {
            var mappedProgramDto = _mapper.Map<ProgramDto>(programViewModel);
            var updatedProgramDto = await _programService.UpdateProgramAsync(mappedProgramDto);
            return Ok(_mapper.Map<ProgramRes>(updatedProgramDto));
        }


        [HttpDelete("{programNo}")]
        public async Task<IActionResult> DeleteProgram(
            string programNo)
        {
            if (string.IsNullOrWhiteSpace(programNo))
                throw new ArgumentException("Program ID cannot be null or empty.", nameof(programNo));

            var isDelete = await _programService.DeleteProgramAsync(programNo);
            if (!isDelete)
            {
                throw new ArgumentException("Program record with ID: {ProgramId} not found for deletion", programNo);
            }
            return Ok(isDelete);
        }
    }
}
