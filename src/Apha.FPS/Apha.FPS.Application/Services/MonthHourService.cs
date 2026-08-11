using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class MonthHourService : IMonthHourService
    {
        private readonly IMonthHourRepository _repository;
        private readonly IMapper _mapper;

        public MonthHourService(IMonthHourRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<MonthHourDto>> GetAllMonthHourAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetAllAsync(parameters);
            return _mapper.Map<PaginatedResult<MonthHourDto>>(pagedData);
        }

        public async Task<IEnumerable<MonthHourDto>> GetMonthHoursByYearAsync(short year)
        {
            var items = await _repository.GetByYearAsync(year);
            return _mapper.Map<IEnumerable<MonthHourDto>>(items);
        }

        public async Task<IEnumerable<short>> GetDistinctYearsAsync()
        {
            return await _repository.GetDistinctYearsAsync();
        }

        public async Task<List<YearEndMonthHourDto>> GetYearEndMonthHoursAsync()
        {
            var items = await _repository.GetYearEndMonthHoursAsync();
            return _mapper.Map<List<YearEndMonthHourDto>>(items);
        }

        public async Task<MonthHourDto> SaveMonthHourAsync(MonthHourDto dto)
        {
            var errors = new List<BusinessValidationError>();

          
            bool hasmissingMissingVal = dto.Days < 0 || dto.VidHours < 0 || dto.CvlHours < 0;

            if (hasmissingMissingVal)
                errors.Add(new BusinessValidationError($"Provided Month Working days, VID hours and CVL hours values are not valid. Values should be non-negative and greater than zero. Please verify.", "Missing_Config"));

         
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);


            var entity = _mapper.Map<MonthHour>(dto);
            var result = await _repository.SaveAsync(entity);
            return _mapper.Map<MonthHourDto>(result);
        }
    }
}
