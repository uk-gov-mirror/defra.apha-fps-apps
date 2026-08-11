using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;
        private readonly IMapper _mapper;

        public ReportService(IReportRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<ReportDto>> GetAllReportsAsync()
        {
            List<Report> entities = await _repository.GetAllReportsAsync();
            return _mapper.Map<List<ReportDto>>(entities);
        }

        public async Task<PaginatedResult<ReportDto>> GetPagedReportsAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedReportsAsync(parameters);
            return _mapper.Map<PaginatedResult<ReportDto>>(pagedData);
        }

       
        public async Task<ReportDto?> GetReportByIdAsync(int id)
        {
            Report? entity = await _repository.GetReportByIdAsync(id);
            return entity is null ? null : _mapper.Map<ReportDto>(entity);
        }

       
        public async Task<ReportDto> CreateReportAsync(ReportDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.ReportName))
                throw new ArgumentException("Report name is required.", nameof(dto));

            Report entity = _mapper.Map<Report>(dto);
            Report created = await _repository.AddReportAsync(entity);
            return _mapper.Map<ReportDto>(created);
        }

        
        public async Task<ReportDto> UpdateReportAsync(ReportDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.ReportName))
                throw new ArgumentException("Report name is required.", nameof(dto));

            bool exists = await _repository.ReportExistsAsync(dto.Id);
            if (!exists)
                throw new KeyNotFoundException($"Report with id {dto.Id} was not found.");

            Report entity = _mapper.Map<Report>(dto);
            Report updated = await _repository.UpdateReportAsync(entity);
            return _mapper.Map<ReportDto>(updated);
        }

        
        public async Task<bool> DeleteReportAsync(int id)
        {
            bool exists = await _repository.ReportExistsAsync(id);
            if (!exists)
                throw new KeyNotFoundException($"Report with id {id} was not found.");

            return await _repository.DeleteReportAsync(id);
        }

        public async Task<bool> ReportExistsAsync(int id)
        {
            return await _repository.ReportExistsAsync(id);
        }
    }
}
