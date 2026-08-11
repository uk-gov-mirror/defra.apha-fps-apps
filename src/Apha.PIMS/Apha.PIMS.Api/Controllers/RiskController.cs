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
    /// <summary>
    /// API controller for Risk Rating lookup maintenance.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/risk-ratings")]
    public class RiskController : ControllerBase
    {
        private readonly IRiskService _service;
        private readonly IMapper _mapper;

        public RiskController(IRiskService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves all risk ratings.</summary>
        /// <returns>Returns <c>200 OK</c> with a full list of <see cref="RiskRes"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllRiskRatings()
        {
            List<RiskDto> result = await _service.GetAllRiskRatingsAsync();
            return Ok(_mapper.Map<List<RiskRes>>(result));
        }

        /// <summary>Retrieves a paged list of risk ratings.</summary>
        /// <param name="query">Paging, sorting and filter parameters.</param>
        /// <returns>Returns <c>200 OK</c> with a paged <see cref="RiskRes"/> collection.</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedRiskRatings([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPagedRiskRatingsAsync(query);
            return Ok(_mapper.Map<PaginationRes<RiskRes>>(result));
        }

        /// <summary>Retrieves a single risk rating by its identifier.</summary>
        /// <param name="riskid">The risk rating identifier.</param>
        /// <returns>Returns <c>200 OK</c> with the matching <see cref="RiskRes"/>, or <c>404 Not Found</c>.</returns>
        [HttpGet("{riskId:int}")]
        public async Task<IActionResult> GetRiskRatingById(int riskId)
        {
            RiskDto? result = await _service.GetRiskRatingByIdAsync(riskId);
            return result is null ? NotFound() : Ok(_mapper.Map<RiskRes>(result));
        }

        /// <summary>Creates a new risk rating.</summary>
        /// <param name="request">The risk rating data to create.</param>
        /// <returns>Returns <c>201 Created</c> with the newly created <see cref="RiskRes"/> and a location header.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateRiskRating([FromBody] RiskReq request)
        {
            RiskDto dto = _mapper.Map<RiskDto>(request);
            RiskDto created = await _service.CreateRiskRatingAsync(dto);
            RiskRes res = _mapper.Map<RiskRes>(created);
            return CreatedAtAction(nameof(GetRiskRatingById), new { riskId = res.Riskid, version = "1.0" }, res);
        }

        /// <summary>Updates an existing risk rating.</summary>
        /// <param name="riskid">The risk rating identifier to update.</param>
        /// <param name="request">The updated risk rating data.</param>
        /// <returns>Returns <c>200 OK</c> with the updated <see cref="RiskRes"/>.</returns>
        [HttpPut("{riskId:int}")]
        public async Task<IActionResult> UpdateRiskRating(int riskId, [FromBody] RiskReq request)
        {
            RiskDto dto = _mapper.Map<RiskDto>(request);
            dto.RiskId = riskId;
            RiskDto updated = await _service.UpdateRiskRatingAsync(dto);
            return Ok(_mapper.Map<RiskRes>(updated));
        }

        /// <summary>Deletes a risk rating by its identifier.</summary>
        /// <param name="riskid">The risk rating identifier to delete.</param>
        /// <returns>Returns <c>200 OK</c> with a success flag, or throws <see cref="KeyNotFoundException"/> if not found.</returns>
        [HttpDelete("{riskId:int}")]
        public async Task<IActionResult> DeleteRiskRating(int riskId)
        {
            bool deleted = await _service.DeleteRiskRatingAsync(riskId);
            return Ok(deleted);
        }
    }
}
