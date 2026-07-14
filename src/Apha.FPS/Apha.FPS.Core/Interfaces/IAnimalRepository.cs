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

        /// <summary>
        /// Returns the global total animal cost across all animal requests for the current FPS year.
        /// Equivalent to the MS Access Form_Activate query used when SellingPC = "ASU":
        /// Sum(NumberOfDays * NumberOfAnimals * DailyRate) JOIN tblAnimals ON AnimalType.
        /// </summary>
        Task<decimal> GetGlobalAnimalCostAsync();
    }
}

