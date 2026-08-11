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
    /// API controller for Publication Type lookup maintenance.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/publication-types")]
    public class PublicationTypeController : ControllerBase
    {
        private readonly IPublicationTypeService _service;
        private readonly IMapper _mapper;

        public PublicationTypeController(IPublicationTypeService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves all publication types.</summary>
        /// <returns>Returns <c>200 OK</c> with a full list of <see cref="PublicationTypeRes"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllPublicationTypes()
        {
            List<PublicationTypeDto> result = await _service.GetAllPublicationTypesAsync();
            return Ok(_mapper.Map<List<PublicationTypeRes>>(result));
        }

        /// <summary>Retrieves a paged list of publication types.</summary>
        /// <param name="query">Paging, sorting and filter parameters.</param>
        /// <returns>Returns <c>200 OK</c> with a paged <see cref="PublicationTypeRes"/> collection.</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedPublicationTypes([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPagedPublicationTypesAsync(query);
            return Ok(_mapper.Map<PaginationRes<PublicationTypeRes>>(result));
        }

        /// <summary>Retrieves a single publication type by its type code.</summary>
        /// <param name="type">The publication type code (string PK, max 3 chars).</param>
        /// <returns>Returns <c>200 OK</c> with the matching <see cref="PublicationTypeRes"/>, or <c>404 Not Found</c>.</returns>
        [HttpGet("{type}")]
        public async Task<IActionResult> GetPublicationTypeByCode(string type)
        {
            PublicationTypeDto? result = await _service.GetPublicationTypeByCodeAsync(type);
            return result is null ? NotFound() : Ok(_mapper.Map<PublicationTypeRes>(result));
        }

        /// <summary>Creates a new publication type.</summary>
        /// <param name="request">The publication type data to create.</param>
        /// <returns>Returns <c>201 Created</c> with the newly created <see cref="PublicationTypeRes"/> and a location header.</returns>
        [HttpPost]
        public async Task<IActionResult> CreatePublicationType([FromBody] PublicationTypeReq request)
        {
            PublicationTypeDto dto = _mapper.Map<PublicationTypeDto>(request);
            PublicationTypeDto created = await _service.CreatePublicationTypeAsync(dto);
            PublicationTypeRes res = _mapper.Map<PublicationTypeRes>(created);
            return CreatedAtAction(nameof(GetPublicationTypeByCode), new { type = res.Type, version = "1.0" }, res);
        }

        /// <summary>Updates an existing publication type.</summary>
        /// <param name="type">The type code to update.</param>
        /// <param name="request">The updated publication type data.</param>
        /// <returns>Returns <c>200 OK</c> with the updated <see cref="PublicationTypeRes"/>.</returns>
        [HttpPut("{type}")]
        public async Task<IActionResult> UpdatePublicationType(string type, [FromBody] PublicationTypeReq request)
        {
            PublicationTypeDto dto = _mapper.Map<PublicationTypeDto>(request);
            dto.Type = type;
            PublicationTypeDto updated = await _service.UpdatePublicationTypeAsync(dto);
            return Ok(_mapper.Map<PublicationTypeRes>(updated));
        }

        /// <summary>Deletes a publication type by its type code.</summary>
        /// <param name="type">The type code to delete.</param>
        /// <returns>Returns <c>200 OK</c> with a success flag, or throws <see cref="KeyNotFoundException"/> if not found.</returns>
        [HttpDelete("{type}")]
        public async Task<IActionResult> DeletePublicationType(string type)
        {
            bool deleted = await _service.DeletePublicationTypeAsync(type);
            return Ok(deleted);
        }
    }
}
