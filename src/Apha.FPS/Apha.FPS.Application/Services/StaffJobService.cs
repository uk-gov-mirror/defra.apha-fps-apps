using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    
    public class StaffJobService : IStaffJobService
    {
        private readonly IStaffJobRepository _staffJobRepository;
        private readonly IMapper _mapper;
        
        public StaffJobService(IStaffJobRepository staffJobRepository, IMapper mapper)
        {
            _staffJobRepository = staffJobRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<StaffJobViewDto>> GetJobStaffCostAsync(QueryParameters<string> queryFilter, string jobCode)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(queryFilter);
            var staffJobViews = await _staffJobRepository.GetJobStaffCostAsync(filter, jobCode);
            return _mapper.Map<PaginatedResult<StaffJobViewDto>>(staffJobViews);
        }

        public async Task<decimal> GetTotalStaffCostAsync(string jobCode)
        {
            return await _staffJobRepository.GetTotalStaffCostAsync(jobCode);
        }

        public async Task<List<StaffWorkgroupLookupDto>> GetStaffWorkgroupLookup()
        {
            var staffWorkgroupLookup = await _staffJobRepository.GetStaffWorkgroupLookup();
            return _mapper.Map<List<StaffWorkgroupLookupDto>>(staffWorkgroupLookup);
        }

        public async Task<StaffWorkgroupLookupDto?> GetStaffSummaryByIdAsync(string staffId)
        {
            var staff = await _staffJobRepository.GetStaffSummaryByIdAsync(staffId);
            return staff == null ? null : _mapper.Map<StaffWorkgroupLookupDto>(staff);
        }

        public async Task<double> GetZtTotalHoursByStaffIdAsync(string staffId)
        {
            return await _staffJobRepository.GetZtTotalHoursByStaffIdAsync(staffId);
        }

        public async Task<PaginatedResult<StaffJobZtViewDto>> GetZtStaffJobsByStaffIdPagedAsync(QueryParameters<string> query, string staffId)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var rows = await _staffJobRepository.GetZtStaffJobsByStaffIdPagedAsync(filter, staffId);
            return _mapper.Map<PaginatedResult<StaffJobZtViewDto>>(rows);
        }

        public async Task<PaginatedResult<StaffJobViewDto>> GetStaffJobsAllocationByJobCodeWgGradePagedAsync(QueryParameters<string> query, string jobcode, string wgGrade)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var rows = await _staffJobRepository.GetStaffJobsAllocationByJobCodeWgGradePagedAsync(filter, jobcode, wgGrade);
            return _mapper.Map<PaginatedResult<StaffJobViewDto>>(rows);
        }

        public async Task<StaffJobZtViewDto?> GetZtStaffJobDetailsByIdAsync(string staffId, string jobCode)
        {
            var result = await _staffJobRepository.GetZtStaffJobDetailsByIdAsync(staffId, jobCode);
            return result == null ? null : _mapper.Map<StaffJobZtViewDto>(result);
        }

        public async Task<decimal?> GetStaffChargeRate(string staffId, string jobcode)
        {
            var chargeRate = await _staffJobRepository.GetStaffChargeRate(staffId, jobcode);
            return chargeRate;
        }

        public async Task<StaffJobViewDto?> GetViewByStaffIdAsync(string staffId, string jobCode)
        {
            var staffWorkgroups = await _staffJobRepository.GetViewByStaffIdAsync(staffId, jobCode);
            return _mapper.Map<StaffJobViewDto>(staffWorkgroups);
        }

        public async Task<StaffJobDto?> GetByIdAsync(string staffId, string jobCode)
        {
            var staffWorkgroup = await _staffJobRepository.GetByIdAsync(staffId, jobCode);
            return _mapper.Map<StaffJobDto>(staffWorkgroup);
        }

        public async Task<StaffJobDto> AddAsync(StaffJobDto staffJob)
        {
            ArgumentNullException.ThrowIfNull(staffJob);
            ArgumentOutOfRangeException.ThrowIfNegative(staffJob.PlannedHours);

            var existing = await _staffJobRepository.GetByIdAsync(staffJob.StaffId, staffJob.JobCode);
            if (existing != null)
                throw new InvalidOperationException($"A staff job entry for staff '" +
                    $"{staffJob.StaffId}' and job code '{staffJob.JobCode}' already exists.");

            var mapStaffJob = _mapper.Map<StaffJob>(staffJob);
            var staffWorkgroup = await _staffJobRepository.AddAsync(mapStaffJob);
            return _mapper.Map<StaffJobDto>(staffWorkgroup);
        }

        public async Task<StaffJobDto> UpdateAsync(StaffJobDto staffJob)
        {
            ArgumentNullException.ThrowIfNull(staffJob);
            ArgumentOutOfRangeException.ThrowIfNegative(staffJob.PlannedHours);

            // If the JobCode has changed (user selected a different ZT code), we need to
            // delete the old record and create a new one because JobCode is part of the composite primary key.
            var lookupJobCode = !string.IsNullOrWhiteSpace(staffJob.OriginalJobCode)
                ? staffJob.OriginalJobCode
                : staffJob.JobCode;

            if (!string.Equals(lookupJobCode, staffJob.JobCode, StringComparison.OrdinalIgnoreCase))
            {
                // Delete the old entry using the original key
                await _staffJobRepository.DeleteAsync(staffJob.StaffId, lookupJobCode);

                // Create a new entry with the new JobCode
                var newStaffJob = _mapper.Map<StaffJob>(staffJob);
                var created = await _staffJobRepository.AddAsync(newStaffJob);
                return _mapper.Map<StaffJobDto>(created);
            }

            var mapStaffJob = _mapper.Map<StaffJob>(staffJob);
            var staffWorkgroup = await _staffJobRepository.UpdateAsync(mapStaffJob);
            return _mapper.Map<StaffJobDto>(staffWorkgroup);
        }

        public async Task<bool> DeleteAsync(string staffId, string jobCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(jobCode);
            var isDeleted = await _staffJobRepository.DeleteAsync(staffId, jobCode);
            return isDeleted;
        }

        public async Task<PaginatedResult<StaffResourceUtilisationDto>> GetStaffResourceUtilisationAsync(QueryParameters<string> query, string workgroup)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _staffJobRepository.GetStaffResourceUtilisationAsync(filter, workgroup);
            return _mapper.Map<PaginatedResult<StaffResourceUtilisationDto>>(result);
        }

    }
}
