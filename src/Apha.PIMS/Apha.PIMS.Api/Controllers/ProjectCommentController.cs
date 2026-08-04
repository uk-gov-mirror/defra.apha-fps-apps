using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectcomment")]
    public class ProjectCommentController : ControllerBase
    {
        private readonly ICommentService _service;
        private readonly IMapper _mapper;

        public ProjectCommentController(ICommentService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetCommentsByProject(
            [FromQuery] string project,
            [FromQuery] int? year,
            [FromQuery] string? topic,
            [FromQuery] PaginationReq<string> query)
        {
            QueryParameters<string> filter = _mapper.Map<QueryParameters<string>>(query);
            PaginatedResult<CommentDto> result = await _service.GetCommentsByProjectAsync(project, year, filter, topic);
            return Ok(_mapper.Map<PaginationRes<CommentRes>>(result));
        }

        
        [HttpGet("{commentno:int}")]
        public async Task<IActionResult> GetById(int commentno)
        {
            CommentDto? result = await _service.GetByIdAsync(commentno);
            if (result is null)
                throw new KeyNotFoundException($"Comment {commentno} not found.");
            return Ok(_mapper.Map<CommentRes>(result));
        }

        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommentReq request)
        {
            CommentDto dto = _mapper.Map<CommentDto>(request);
            CommentDto result = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { commentno = result.CommentNo }, _mapper.Map<CommentRes>(result));
        }

        
        [HttpPut("{commentno:int}")]
        public async Task<IActionResult> Update(int commentno, [FromBody] CommentReq request)
        {
            CommentDto dto = _mapper.Map<CommentDto>(request);
            dto.CommentNo = commentno;
            CommentDto result = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<CommentRes>(result));
        }

        
        [HttpDelete("{commentno:int}")]
        public async Task<IActionResult> Delete(int commentno)
        {
            bool deleted = await _service.DeleteAsync(commentno);
            return Ok(deleted);
        }

        
        [HttpGet("commenttopics")]
        public async Task<IActionResult> GetCommentTopics()
        {
            IEnumerable<CommentTopicDto> topics = await _service.GetCommentTopicsAsync();
            return Ok(_mapper.Map<IEnumerable<CommentTopicRes>>(topics));
        }

        [HttpGet("forecastspend")]
        public async Task<IActionResult> GetForecastSpendByProject([FromQuery] string project)
        {
            double? forecastSpend = await _service.GetForecastSpendByProjectAsync(project);
            return Ok(new ProjectCommentForecastSpendRes { ForecastSpend = forecastSpend });
        }

        [HttpPut("forecastspend")]
        public async Task<IActionResult> UpdateForecastSpendByProject([FromQuery] string project, [FromBody] ProjectCommentForecastSpendRes request)
        {
            if (string.IsNullOrWhiteSpace(project))
                return BadRequest("Project is required.");

            if (request is null)
                return BadRequest("Forecast spend payload is required.");

            double? forecastSpend = await _service.UpdateForecastSpendByProjectAsync(project, request.ForecastSpend);
            return Ok(new ProjectCommentForecastSpendRes { ForecastSpend = forecastSpend });
        }
    }
}
