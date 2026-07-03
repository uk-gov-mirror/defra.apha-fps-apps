/*
 * TRANSFORMENGINE MIGRATION — IAnimalRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - Added GetAnimalCostByAnimalTypeAsync(PaginationParameters<string> query, string animalType)
 *     to support ASU View: filter AnimalCostView rows by animal type rather than job code
 *
 * PRESERVED:
 *   - All existing Animal Master CRUD signatures
 *   - All existing Animal Cost (AnimalJob) signatures
 *   - Namespace, using directives, and interface name
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: AnimalRepository implementation must add corresponding method body
 *     to back this new interface signature (Phase 4 DataAccess work)
 */

using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IAnimalRepository
    {
        // Animal Master CRUD
        Task<IEnumerable<Animal>> GetAllAnimalsAsync();
        Task<PagedData<Animal>> GetAllAnimalsAsync(PaginationParameters<string> query);
        Task<Animal?> GetAnimalByIdAsync(string animalType);
        Task<Animal> AddAnimalAsync(Animal entity);
        Task<Animal> UpdateAnimalAsync(Animal entity);
        Task<bool> DeleteAnimalAsync(string animalType);

        // Animal Cost (AnimalJob)
        Task<List<Animal>> GetAnimalLookup();
        Task<PagedData<AnimalCostView>> GetAnimalCostAsync(PaginationParameters<string> query, string jobCode);
        Task<decimal> GetTotalAnimalCostAsync(string jobCode);
        Task<AnimalCostView?> GetAnimalCostViewByIdAsync(int indCounter, string jobCode);
        Task<decimal?> GetAnimalRateByIdAsync(string animalType, string jobCode);
        Task<AnimalRequest> AddAnimalCostAsync(AnimalRequest animalReq);
        Task<AnimalRequest> UpdateAnimalCostAsync(AnimalRequest animalReq);
        Task<bool> DeleteJobAnimalCostAsync(int indCounter);

        // TRANSFORMENGINE: New method added for ASU View — query AnimalCostView filtered by animalType
        // rather than jobCode; mirrors GetAnimalCostAsync signature with animalType discriminator
        Task<PagedData<AnimalCostView>> GetAnimalCostByAnimalTypeAsync(PaginationParameters<string> query, string animalType);
    }
}

