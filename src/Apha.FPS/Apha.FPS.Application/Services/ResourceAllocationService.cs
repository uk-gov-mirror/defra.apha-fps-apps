using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for Stage 2 Check Resource Allocation
    /// (frmResourceAllocation) read-only grid data.
    /// </summary>
    public class ResourceAllocationService : IResourceAllocationService
    {
        private readonly IResourceAllocationRepository _repository;
        private readonly IMapper _mapper;

        public ResourceAllocationService(IResourceAllocationRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaginatedResult<ResourceStaffAllocationDto>> GetPagedStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade, QueryParameters<string> query)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workGroupGrade);
            ArgumentNullException.ThrowIfNull(query);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var paged = await _repository.GetPagedStaffAllocationsByWorkGroupGradeAsync(workGroupGrade, filter);
            return _mapper.Map<PaginatedResult<ResourceStaffAllocationDto>>(paged);
        }

        public async Task<PaginatedResult<ResourceStaffJobDetailDto>> GetPagedStaffJobDetailsByStaffIdAsync(string staffId, QueryParameters<string> query)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(staffId);
            ArgumentNullException.ThrowIfNull(query);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var paged = await _repository.GetPagedStaffJobDetailsByStaffIdAsync(staffId, filter);
            return _mapper.Map<PaginatedResult<ResourceStaffJobDetailDto>>(paged);
        }
    }
}
