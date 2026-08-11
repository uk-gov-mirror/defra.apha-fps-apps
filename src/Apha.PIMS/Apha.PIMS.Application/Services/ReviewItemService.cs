using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class ReviewItemService : IReviewItemService
    {
        private readonly IReviewItemRepository _repository;
        private readonly IMapper _mapper;

        public ReviewItemService(IReviewItemRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        
        public async Task<List<ReviewItemDto>> GetAllReviewItemsAsync()
        {
            List<ReviewItem> entities = await _repository.GetAllReviewItemsAsync();
            return _mapper.Map<List<ReviewItemDto>>(entities);
        }

       
        public async Task<PaginatedResult<ReviewItemDto>> GetPagedReviewItemsAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedReviewItemsAsync(parameters);
            return _mapper.Map<PaginatedResult<ReviewItemDto>>(pagedData);
        }

       
        public async Task<ReviewItemDto?> GetReviewItemByIdAsync(int itemId)
        {
            ReviewItem? entity = await _repository.GetReviewItemByIdAsync(itemId);
            return entity is null ? null : _mapper.Map<ReviewItemDto>(entity);
        }

       
        public async Task<ReviewItemDto> CreateReviewItemAsync(ReviewItemDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool duplicate = await _repository.ReviewItemExistsAsync(dto.ItemId);
            if (duplicate)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"A review item with ID '{dto.ItemId}' already exists.",
                        "REVIEW_ITEM_DUPLICATE")
                ]);

            ReviewItem entity = _mapper.Map<ReviewItem>(dto);
            ReviewItem created = await _repository.AddReviewItemAsync(entity);
            return _mapper.Map<ReviewItemDto>(created);
        }

       
        public async Task<ReviewItemDto> UpdateReviewItemAsync(ReviewItemDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool exists = await _repository.ReviewItemExistsAsync(dto.ItemId);
            if (!exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"ReviewItem with itemid {dto.ItemId} was not found.",
                        "REVIEW_ITEM_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            ReviewItem entity = _mapper.Map<ReviewItem>(dto);
            ReviewItem updated = await _repository.UpdateReviewItemAsync(entity);
            return _mapper.Map<ReviewItemDto>(updated);
        }

        
        public async Task<bool> DeleteReviewItemAsync(int itemId)
        {
            bool exists = await _repository.ReviewItemExistsAsync(itemId);
            if (!exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"ReviewItem with itemid {itemId} was not found.",
                        "REVIEW_ITEM_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            return await _repository.DeleteReviewItemAsync(itemId);
        }

        public async Task<bool> ReviewItemExistsAsync(int itemId)
        {
            return await _repository.ReviewItemExistsAsync(itemId);
        }
    }
}
