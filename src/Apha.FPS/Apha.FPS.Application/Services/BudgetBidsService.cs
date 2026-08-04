using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class BudgetBidsService : IBudgetBidsService
    {
        private readonly IBudgetBidsRepository _repository;
        private readonly IMapper _mapper;

        public BudgetBidsService(IBudgetBidsRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper     = mapper     ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<BidViewDto>> GetBidViewAsync(string workgroup)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workgroup);
            var entities = await _repository.GetBidViewAsync(workgroup);
            return _mapper.Map<List<BidViewDto>>(entities);
        }

        public async Task<PaginatedResult<BidViewDto>> GetBidViewPagedAsync(QueryParameters<string> query, string workgroup)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workgroup);
            var parameters = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _repository.GetBidViewPagedAsync(parameters, workgroup);
            return _mapper.Map<PaginatedResult<BidViewDto>>(pagedData);
        }

        public async Task<BidDto?> GetBidByIdAsync(string WorkGroupName, string account)
        {
            var entity = await _repository.GetBidByIdAsync(WorkGroupName, account);
            return _mapper.Map<BidDto>(entity);
        }

        public Task<BidDto> AddBidAsync(BidDto bid)
        {
            ArgumentNullException.ThrowIfNull(bid);
            ArgumentOutOfRangeException.ThrowIfNegative(bid.GenBid);
            return AddBidAsyncCore(bid);
        }

        private async Task<BidDto> AddBidAsyncCore(BidDto bid)
        {
            var existing = await _repository.GetBidByIdAsync(bid.WorkGroupName, bid.Account);
            if (existing != null)
                throw new InvalidOperationException("Account already exists.");

            var entity = _mapper.Map<Bid>(bid);
            var result = await _repository.AddBidAsync(entity);
            return _mapper.Map<BidDto>(result);
        }

        public Task<BidDto> UpdateBidAsync(BidDto bid)
        {
            ArgumentNullException.ThrowIfNull(bid);
            ArgumentOutOfRangeException.ThrowIfNegative(bid.GenBid);
            return UpdateBidAsyncCore(bid);
        }

        private async Task<BidDto> UpdateBidAsyncCore(BidDto bid)
        {
            var existing = await _repository.GetBidByIdAsync(bid.WorkGroupName, bid.Account);
            if (existing == null)
                throw new InvalidOperationException(
                    $"Bid with Workgroup '{bid.WorkGroupName}' and Account '{bid.Account}' was not found.");

            var entity = _mapper.Map<Bid>(bid);
            var result = await _repository.UpdateBidAsync(entity);
            return _mapper.Map<BidDto>(result);
        }

        public Task<bool> DeleteBidAsync(string WorkGroupName, string account)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(WorkGroupName);
            return DeleteBidAsyncCore(WorkGroupName, account);
        }

        private async Task<bool> DeleteBidAsyncCore(string WorkGroupName, string account)
        {
            var hasRelatedPurchases = await _repository.HasRelatedPurchasesAsync(WorkGroupName, account);
            if (hasRelatedPurchases)
                throw new InvalidOperationException(
                    "This record cannot be deleted as it has a related entry in the Purchase table.");

            return await _repository.DeleteBidAsync(WorkGroupName, account);
        }

        public async Task<List<AccountCategoryDto>> GetAccountCategoriesAsync()
        {
            var categories = await _repository.GetAccountCategoriesAsync();
            return _mapper.Map<List<AccountCategoryDto>>(categories);
        }

        public async Task<PaginatedResult<GenericBidViewDto>> GetGenericBidsPagedAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _repository.GetGenericBidsPagedAsync(parameters);
            return _mapper.Map<PaginatedResult<GenericBidViewDto>>(pagedData);
        }
    }
}
