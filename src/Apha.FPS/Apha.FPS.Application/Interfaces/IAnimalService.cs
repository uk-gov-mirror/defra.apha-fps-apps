/*
 * TRANSFORMENGINE MIGRATION — IAnimalService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - Added GetAnimalCostByAnimalTypeAsync(QueryParameters<string> query, string animalType)
 *     to support the ASU View resource family — filters AnimalCostView rows by animal type
 *     rather than job code, as inferred from fps_asuview.js filter logic
 *
 * PRESERVED:
 *   - All existing Animal Master CRUD signatures (GetAllAnimalsAsync, GetAnimalByIdAsync,
 *     AddAnimalAsync, UpdateAnimalAsync, DeleteAnimalAsync)
 *   - All existing Animal Cost (AnimalJob) signatures (GetAnimalCostAsync, GetTotalAnimalCostAsync,
 *     GetAnimalCostViewByIdAsync, GetAnimalRateByIdAsync, AddAnimalCostAsync, UpdateAnimalCostAsync,
 *     DeleteAnimalCostAsync, GetAnimalLookupAsync)
 *   - Namespace, using directives, and interface name
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IAnimalService
    {
        // Animal Master CRUD
        Task<IEnumerable<AnimalDto>> GetAllAnimalsAsync();
        Task<PaginatedResult<AnimalDto>> GetAllAnimalsAsync(QueryParameters<string> query);
        Task<AnimalDto?> GetAnimalByIdAsync(string animalType);
        Task<AnimalDto> AddAnimalAsync(AnimalDto animalDto);
        Task<AnimalDto> UpdateAnimalAsync(AnimalDto animalDto);
        Task<bool> DeleteAnimalAsync(string animalType);

        // Animal Cost (AnimalJob)
        Task<List<AnimalDto>> GetAnimalLookupAsync();
        Task<PaginatedResult<AnimalCostViewDto>> GetAnimalCostAsync(QueryParameters<string> query, string jobCode);
        Task<decimal> GetTotalAnimalCostAsync(string jobCode);
        Task<AnimalCostViewDto?> GetAnimalCostViewByIdAsync(int indCounter, string jobCode);
        Task<decimal?> GetAnimalRateByIdAsync(string animalType, string jobCode);
        Task<AnimalRequestDto> AddAnimalCostAsync(AnimalRequestDto animalReq);
        Task<AnimalRequestDto> UpdateAnimalCostAsync(AnimalRequestDto animalReq);
        Task<bool> DeleteAnimalCostAsync(int indCounter);

        // TRANSFORMENGINE: New method for ASU View — filter AnimalCostView by animalType
        // mirrors GetAnimalCostAsync but discriminates by animalType instead of jobCode
        Task<PaginatedResult<AnimalCostViewDto>> GetAnimalCostByAnimalTypeAsync(QueryParameters<string> query, string animalType);
    }
}

