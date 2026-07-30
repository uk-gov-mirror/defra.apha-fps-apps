using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class AdditionalCostService : IAdditionalCostService
    {
        private readonly IAdditionalCostRepository _repository;
        private readonly IMapper _mapper;

        public AdditionalCostService(IAdditionalCostRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<AdditionalCostDto>> GetByJobCodeAsync(QueryParameters<string> queryFilter, string jobCode)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(queryFilter);
            var data = await _repository.GetByJobCodeAsync(filter, jobCode);
            return _mapper.Map<PaginatedResult<AdditionalCostDto>>(data);
        }

        public async Task<decimal> GetTotalItemCostAsync(string jobCode)
        {
            return await _repository.GetTotalItemCostAsync(jobCode);
        }

        public async Task<List<AccountCategoryDto>> GetAccountCategoriesAsync()
        {
            var categories = await _repository.GetAccountCategoriesAsync();
            return _mapper.Map<List<AccountCategoryDto>>(categories);
        }

        public async Task<AdditionalCostDto?> GetByIdAsync(string jobCode, string account, string description)
        {
            var entity = await _repository.GetByIdAsync(jobCode, account, description);
            return _mapper.Map<AdditionalCostDto>(entity);
        }

        public async Task<AdditionalCostDto> AddAsync(AdditionalCostDto additionalCost)
        {
            ArgumentNullException.ThrowIfNull(additionalCost);
            ArgumentOutOfRangeException.ThrowIfNegative(additionalCost.ItemCost);

            var existing = await _repository.GetByIdAsync(
                additionalCost.JobCode, additionalCost.Account, additionalCost.Description);

            if (existing != null)
                throw new InvalidOperationException(
                    $"An additional cost with Job Code '{additionalCost.JobCode}', Account '{additionalCost.Account}' and Description '{additionalCost.Description}' already exists.");

            var entity = _mapper.Map<AdditionalCost>(additionalCost);
            var result = await _repository.AddAsync(entity);
            return _mapper.Map<AdditionalCostDto>(result);
        }

        public async Task<AdditionalCostDto> UpdateAsync(AdditionalCostDto additionalCost)
        {
            ArgumentNullException.ThrowIfNull(additionalCost);
            ArgumentOutOfRangeException.ThrowIfNegative(additionalCost.ItemCost);

            var originalDescription = string.IsNullOrWhiteSpace(additionalCost.OriginalDescription)
                ? additionalCost.Description
                : additionalCost.OriginalDescription;

            var originalAccount = string.IsNullOrWhiteSpace(additionalCost.OriginalAccount)
                ? additionalCost.Account
                : additionalCost.OriginalAccount;

            var existing = await _repository.GetByIdAsync(
                additionalCost.JobCode, originalAccount, originalDescription);

            if (existing == null)
                throw new InvalidOperationException(
                    $"Additional cost with Job Code '{additionalCost.JobCode}', Account '{originalAccount}' and Description '{originalDescription}' was not found.");

            var descriptionChanged = !string.Equals(
                originalDescription, additionalCost.Description, StringComparison.OrdinalIgnoreCase);

            var accountChanged = !string.Equals(
                originalAccount, additionalCost.Account, StringComparison.OrdinalIgnoreCase);

            if (descriptionChanged || accountChanged)
            {
                var duplicate = await _repository.GetByIdAsync(
                    additionalCost.JobCode, additionalCost.Account, additionalCost.Description);

                if (duplicate != null)
                    throw new InvalidOperationException(
                        $"An additional cost with Job Code '{additionalCost.JobCode}', Account '{additionalCost.Account}' and Description '{additionalCost.Description}' already exists.");
            }

            var entity = _mapper.Map<AdditionalCost>(additionalCost);
            var result = await _repository.UpdateAsync(entity, originalAccount, originalDescription);
            return _mapper.Map<AdditionalCostDto>(result);
        }

        public async Task<bool> DeleteAsync(string jobCode, string account, string description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(jobCode);
            return await _repository.DeleteAsync(jobCode, account, description);
        }
    }
}
