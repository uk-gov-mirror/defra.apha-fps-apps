/*
 * TRANSFORMENGINE MIGRATION — AnimalController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - Added GET asu-view endpoint: GetAsuViewAsync([FromQuery] QueryParameters<string> query, [FromQuery] string? animalType)
 *     Returns PaginationRes<AsuViewRes> filtered by animalType, delegating to IAnimalService.GetAnimalCostByAnimalTypeAsync
 *   - Route: GET /api/v1/animal/asu-view?animalType=X (literal segment takes precedence over {animalType} param route)
 *   - Missing animalType argument throws ArgumentException (mapped to 400 by ExceptionMiddleware)
 *
 * PRESERVED:
 *   - All existing Animal Master CRUD actions (GetAllAnimalsAsync, GetAllAnimalsPagedAsync,
 *     GetAnimalByIdAsync, CreateAnimal, UpdateAnimal, DeleteAnimal)
 *   - Class-level [Authorize], [Route], [ApiController], [ApiVersion] attributes
 *   - Constructor null-guards, injected services (_animalService, _mapper)
 *   - All XML summary docs on existing actions
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE RESOLVED (Phase 14): [Authorize] roles confirmed — class-level
 *     [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")] covers the asu-view
 *     endpoint and is consistent with 15+ other FPS backend controllers. API-FPSShared is
 *     the FPS read-only role. No change required.
 *   - TRANSFORMENGINE TODO: confirm whether animalType=null should return all records or
 *     reject with 400 — currently throws ArgumentException (400) when null/empty
 */
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
    /// Controller for managing Animal Master (tblAnimals_MAP) CRUD operations
    /// and the ASU View (Animal Species Usage) aggregated cost endpoint.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/animal")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AnimalController : ControllerBase
    {
        private readonly IAnimalService _animalService;
        private readonly IMapper _mapper;

        public AnimalController(IAnimalService animalService, IMapper mapper)
        {
            _animalService = animalService ?? throw new ArgumentNullException(nameof(animalService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>Gets all animals.</summary>
        [HttpGet]
        public async Task<ActionResult> GetAllAnimalsAsync()
        {
            var dtos = await _animalService.GetAllAnimalsAsync();
            return Ok(_mapper.Map<List<AnimalRes>>(dtos));
        }

        /// <summary>Gets a paged list of animals.</summary>
        [HttpGet("paged")]
        public async Task<ActionResult> GetAllAnimalsPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var paged = await _animalService.GetAllAnimalsAsync(query);
            return Ok(_mapper.Map<PaginationRes<AnimalRes>>(paged));
        }

        // TRANSFORMENGINE: GET asu-view — new endpoint for ASU View resource family.
        // Literal route segment "asu-view" takes precedence over the parameterized {animalType}
        // route below; no routing conflict. Delegates to GetAnimalCostByAnimalTypeAsync added to
        // IAnimalService in Phase 3 and AnimalRepository in Phase 4.
        /// <summary>
        /// Gets a paged list of animal cost records filtered by animal type (ASU View).
        /// Route: GET /api/v1/animal/asu-view?animalType=X
        /// Lookup: GET /api/v1/animal returns all animal types with DailyRate for the dropdown.
        /// </summary>
        /// <param name="query">Pagination, sorting, and search parameters.</param>
        /// <param name="animalType">Required. The animal type to filter by (maps to the Animal Type dropdown in fps_asuview.html).</param>
        [HttpGet("asu-view")]
        public async Task<ActionResult> GetAsuViewAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string? animalType)
        {
            // TRANSFORMENGINE: validate animalType — ArgumentException maps to HTTP 400 via ExceptionMiddleware
            if (string.IsNullOrWhiteSpace(animalType))
                throw new ArgumentException("animalType query parameter is required.", nameof(animalType));

            var paged = await _animalService.GetAnimalCostByAnimalTypeAsync(query, animalType);
            return Ok(_mapper.Map<PaginationRes<AsuViewRes>>(paged));
        }

        /// <summary>Gets an animal by its type key.</summary>
        [HttpGet("{animalType}")]
        public async Task<ActionResult<AnimalRes>> GetAnimalByIdAsync(string animalType)
        {
            var dto = await _animalService.GetAnimalByIdAsync(animalType);
            if (dto == null)
                throw new ArgumentException($"Animal '{animalType}' not found.");
            return Ok(_mapper.Map<AnimalRes>(dto));
        }

        /// <summary>Creates a new animal master record.</summary>
        [HttpPost]
        public async Task<ActionResult<AnimalRes>> CreateAnimal([FromBody] AnimalReq req)
        {
            var dto = _mapper.Map<AnimalDto>(req);
            var added = await _animalService.AddAnimalAsync(dto);
            return Ok(_mapper.Map<AnimalRes>(added));
        }

        /// <summary>Updates an existing animal master record.</summary>
        [HttpPut]
        public async Task<ActionResult<AnimalRes>> UpdateAnimal([FromBody] AnimalReq req)
        {
            var dto = _mapper.Map<AnimalDto>(req);
            var updated = await _animalService.UpdateAnimalAsync(dto);
            return Ok(_mapper.Map<AnimalRes>(updated));
        }

        /// <summary>Deletes an animal master record.</summary>
        [HttpDelete("{animalType}")]
        public async Task<IActionResult> DeleteAnimal(string animalType)
        {
            if (string.IsNullOrWhiteSpace(animalType))
                throw new ArgumentException("Animal type cannot be null or empty.", nameof(animalType));

            var deleted = await _animalService.DeleteAnimalAsync(animalType);
            if (!deleted)
                throw new ArgumentException($"Animal '{animalType}' not found.");
            return Ok(deleted);
        }
    }
}
