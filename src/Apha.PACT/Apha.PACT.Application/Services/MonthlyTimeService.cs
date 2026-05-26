using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class MonthlyTimeService : IMonthlyTimeService
    {
        private readonly IMonthlyTimeRepository _repository;
        private readonly IMapper _mapper;

        public MonthlyTimeService(IMonthlyTimeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MonthlyTimeDto>> GetMonthlyTimeByTimeCodeAndProjectAsync(string timeCode, string workGroup, string parentProject)
        {
            var items = await _repository.GetMonthlyTimeByTimeCodeAndProjectAsync(timeCode, workGroup, parentProject);
            return _mapper.Map<IEnumerable<MonthlyTimeDto>>(items);
        }

        public async Task<PaginatedResult<MonthlyTimeDto>> GetPagedMonthlyTimeAsync(QueryParameters<string> query, string? timeCode, string? workGroup, string? parentProject)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedMonthlyTimeAsync(parameters, timeCode, workGroup, parentProject);
            return _mapper.Map<PaginatedResult<MonthlyTimeDto>>(pagedData);
        }

        public async Task<MonthlyTimeDto?> GetMonthlyTimeByIdAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var item = await _repository.GetMonthlyTimeByIdAsync(pactStaffId, timeCode, month, parentProject);
            return item == null ? null : _mapper.Map<MonthlyTimeDto>(item);
        }

        public async Task<MonthlyTimeDto> CreateMonthlyTimeAsync(MonthlyTimeDto dto)
        {
            var entity = _mapper.Map<MonthlyTime>(dto);
            var created = await _repository.CreateMonthlyTimeAsync(entity);
            return _mapper.Map<MonthlyTimeDto>(created);
        }

        public async Task<MonthlyTimeDto> UpdateMonthlyTimeAsync(MonthlyTimeDto dto)
        {
            var entity = _mapper.Map<MonthlyTime>(dto);
            var updated = await _repository.UpdateMonthlyTimeAsync(entity);
            return _mapper.Map<MonthlyTimeDto>(updated);
        }

        public async Task<bool> DeleteMonthlyTimeAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            return await _repository.DeleteMonthlyTimeAsync(pactStaffId, timeCode, month, parentProject);
        }
    }
}
