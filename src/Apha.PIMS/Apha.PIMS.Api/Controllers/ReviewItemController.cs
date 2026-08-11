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
    [Route("api/v{version:apiVersion}/reviewitem")]
    public class ReviewItemController : ControllerBase
    {
        private readonly IReviewItemService _service;
        private readonly IMapper _mapper;

        public ReviewItemController(IReviewItemService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all review items.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllReviewItems()
        {
            
            List<ReviewItemDto> result = await _service.GetAllReviewItemsAsync();
            return Ok(_mapper.Map<List<ReviewItemRes>>(result));
        }

        /// <summary>Get paged review items.</summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedReviewItems([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetPagedReviewItemsAsync(query);
            return Ok(_mapper.Map<PaginationRes<ReviewItemRes>>(result));
        }

        /// <summary>Get a single review item by itemid.</summary>
        [HttpGet("{itemId:int}")]
        public async Task<IActionResult> GetReviewItemById(int itemId)
        {
            ReviewItemDto? result = await _service.GetReviewItemByIdAsync(itemId);
            return result is null ? NotFound() : Ok(_mapper.Map<ReviewItemRes>(result));
        }

        /// <summary>Create a new review item.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateReviewItem([FromBody] ReviewItemReq request)
        {
            ReviewItemDto dto = _mapper.Map<ReviewItemDto>(request);
            ReviewItemDto created = await _service.CreateReviewItemAsync(dto);
            ReviewItemRes res = _mapper.Map<ReviewItemRes>(created);
            return CreatedAtAction(nameof(GetReviewItemById), new { itemId = res.ItemId, version = "1.0" }, res);
        }

        /// <summary>Update an existing review item.</summary>
        [HttpPut("{itemId:int}")]
        public async Task<IActionResult> UpdateReviewItem(int itemId, [FromBody] ReviewItemReq request)
        {
            ReviewItemDto dto = _mapper.Map<ReviewItemDto>(request);
            dto.ItemId = itemId;
            ReviewItemDto updated = await _service.UpdateReviewItemAsync(dto);
            return Ok(_mapper.Map<ReviewItemRes>(updated));
        }

        /// <summary>Delete a review item by itemid.</summary>
        [HttpDelete("{itemId:int}")]
        public async Task<IActionResult> DeleteReviewItem(int itemId)
        {
            bool deleted = await _service.DeleteReviewItemAsync(itemId);
            return Ok(deleted);
        }
    }
}
