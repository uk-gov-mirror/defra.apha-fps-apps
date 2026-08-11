using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class TimeCodeValidService : ITimeCodeValidService
    {
        private readonly ITimeCodeValidRepository _repository;
        private readonly IJobCodeRepository _jobCodeRepository;
        private readonly ITestCapabilityRepository _testCapabilityRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMonthlyTimeRepository _monthlyTimeRepository;
        private readonly IMapper _mapper;

        public TimeCodeValidService(
            ITimeCodeValidRepository repository,
            IJobCodeRepository jobCodeRepository,
            ITestCapabilityRepository testCapabilityRepository,
            IProjectRepository projectRepository,
            IMonthlyTimeRepository monthlyTimeRepository,
            IMapper mapper)
        {
            _repository = repository;
            _jobCodeRepository = jobCodeRepository;
            _testCapabilityRepository = testCapabilityRepository;
            _projectRepository = projectRepository;
            _monthlyTimeRepository = monthlyTimeRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TimeCodeValidDto>> GetByJobCodeAsync(string jobCode, string parentProject)
        {
            var items = await _repository.GetByJobCodeAsync(jobCode, parentProject);
            return _mapper.Map<IEnumerable<TimeCodeValidDto>>(items);
        }

        public async Task<IEnumerable<TimeCodeValidDto>> GetTimeCodeValidsByWorkGroupAsync(string workGroup)
        {
            var items = await _repository.GetTimeCodeValidsByWorkGroupAsync(workGroup);
            return _mapper.Map<IEnumerable<TimeCodeValidDto>>(items);
        }

        public async Task<IEnumerable<string>> GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync(string workGroup, string timeCode)
        {
            var items = await _repository.GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync(workGroup, timeCode);
            return items;
        }

        public async Task<IEnumerable<string>> GetAllDistinctTimeCodesAsync()
        {
            var items = await _repository.GetTimeCodeValidsAsync();
            return items.Select(x => x.TimeCode).Distinct().OrderBy(x => x);
        }

        public async Task<IEnumerable<string>> GetAllDistinctProjectsAsync()
        {
            var items = await _repository.GetTimeCodeValidsAsync();
            return items.Select(x => x.ParentProject).Distinct().OrderBy(x => x);
        }

        public async Task<PaginatedResult<TimeCodeValidDto>> GetPagedTimeCodesAsync(QueryParameters<string> query, string? jobCode, string? parentProject)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedTimeCodesAsync(parameters, jobCode, parentProject);
            return _mapper.Map<PaginatedResult<TimeCodeValidDto>>(pagedData);
        }

        public async Task<PaginatedResult<TimeCodeValidDto>> GetPagedByProjectAndTestCodeAsync(QueryParameters<string> query, string parentProject, string testCode)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedByProjectAndTestCodeAsync(parameters, parentProject, testCode);
            return _mapper.Map<PaginatedResult<TimeCodeValidDto>>(pagedData);
        }

        public async Task<TimeCodeValidDto?> GetTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
        {
            var item = await _repository.GetTimeCodeValidAsync(workGroup, timeCode, parentProject);
            return item == null ? null : _mapper.Map<TimeCodeValidDto>(item);
        }

        public async Task<TimeCodeValidDto> CreateTimeCodeValidAsync(TimeCodeValidDto timeCodeValid)
        {
            await ValidateTimeCodeFieldsAsync(timeCodeValid, null);
            var entity = _mapper.Map<TimeCodeValid>(timeCodeValid);
            var created = await _repository.CreateTimeCodeValidAsync(entity);
            return _mapper.Map<TimeCodeValidDto>(created);
        }

        public async Task<TimeCodeValidDto> UpdateTimeCodeValidAsync(TimeCodeValidDto timeCodeValid)
        {
            var existing = await _repository.GetTimeCodeValidAsync(
                timeCodeValid.WorkGroup, timeCodeValid.TimeCode, timeCodeValid.ParentProject);
            await ValidateTimeCodeFieldsAsync(timeCodeValid, existing);
            var entity = _mapper.Map<TimeCodeValid>(timeCodeValid);
            var updated = await _repository.UpdateTimeCodeValidAsync(entity);
            return _mapper.Map<TimeCodeValidDto>(updated);
        }

        public async Task<bool> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
        {
            return await _repository.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);
        }

        public async Task<bool> DeleteAllByJobCodeAsync(string jobCode, string parentProject)
        {
            return await _repository.DeleteAllByJobCodeAsync(jobCode, parentProject);
        }

        public async Task<IEnumerable<TimeCodeValidDto>> CopyWorkGroupAsync(string sourceJobCode, string targetJobCode, string parentProject)
        {
            var items = await _repository.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);
            return _mapper.Map<IEnumerable<TimeCodeValidDto>>(items);
        }

        public async Task<bool> DeleteBulkAsync(IEnumerable<(string WorkGroup, string TimeCode)> items, string parentProject)
        {
            return await _repository.DeleteBulkAsync(items, parentProject);
        }

        public async Task<IEnumerable<TimeCodeValidDto>> CopySelectedWorkGroupsAsync(IEnumerable<string> workGroups, string sourceJobCode, string targetJobCode, string parentProject)
        {
            var result = await _repository.CopySelectedWorkGroupsAsync(workGroups, sourceJobCode, targetJobCode, parentProject);
            return _mapper.Map<IEnumerable<TimeCodeValidDto>>(result);
        }

        /// <summary>
        /// Validates FK combinations mirroring TimeCodeValid_ITrig (insert) and TimeCodeValid_UTrig (update).
        /// Pass <paramref name="existing"/> as null for insert validation; supply existing entity for update validation.
        /// </summary>
        private async Task ValidateTimeCodeFieldsAsync(TimeCodeValidDto dto, TimeCodeValid? existing)
        {
            ValidateRequiredFieldCombination(dto);

            bool isInsert = existing == null;

            await ValidateJobCodeAsync(dto, isInsert, existing);
            await ValidateMonthlyTimeDependencyAsync(dto, isInsert, existing);
            await ValidateTestCapabilityAsync(dto, isInsert, existing);
            await ValidateParentProjectAsync(dto, isInsert, existing);
            await ValidateDuplicateAsync(dto, isInsert);
        }

        private static void ValidateRequiredFieldCombination(TimeCodeValidDto dto)
        {
            bool hasTestCode = !string.IsNullOrEmpty(dto.TestCode);
            bool hasPortfolio = !string.IsNullOrEmpty(dto.Portfolio);
            bool hasJobCode = !string.IsNullOrEmpty(dto.JobCode);

            bool hasTestAndPortfolio = hasTestCode && hasPortfolio;
            bool hasPartialTestPortfolio = hasTestCode != hasPortfolio;

            if (!hasJobCode && !hasTestAndPortfolio || hasPartialTestPortfolio)
                throw new InvalidOperationException("Must fill in Testcode and Portfolio, or Jobcode");
        }

        private async Task ValidateJobCodeAsync(TimeCodeValidDto dto, bool isInsert, TimeCodeValid? existing)
        {
            if (string.IsNullOrEmpty(dto.JobCode))
                return;

            if (!isInsert && existing!.JobCode == dto.JobCode)
                return;

            var jobCode = await _jobCodeRepository.GetJobCodeByIdAsync(dto.JobCode);
            if (jobCode == null)
                throw new InvalidOperationException("Not a valid jobcode.");
        }

        private async Task ValidateMonthlyTimeDependencyAsync(TimeCodeValidDto dto, bool isInsert, TimeCodeValid? existing)
        {
            if (isInsert)
                return;

            bool workGroupChanged = existing!.WorkGroup != dto.WorkGroup;
            bool timeCodeChanged = existing.TimeCode != dto.TimeCode;
            bool parentProjectChanged = existing.ParentProject != dto.ParentProject;

            if (!workGroupChanged && !timeCodeChanged && !parentProjectChanged)
                return;

            var hasDependentRows = await _monthlyTimeRepository.HasMonthlyTimeEntriesAsync(
                existing.WorkGroup, existing.TimeCode, existing.ParentProject);

            if (hasDependentRows)
                throw new InvalidOperationException("Cannot update, existing data in MonthlyTime.");
        }

        private async Task ValidateTestCapabilityAsync(TimeCodeValidDto dto, bool isInsert, TimeCodeValid? existing)
        {
            if (string.IsNullOrEmpty(dto.TestCode) || string.IsNullOrEmpty(dto.Portfolio))
                return;

            if (!isInsert && existing!.TestCode == dto.TestCode && existing.Portfolio == dto.Portfolio)
                return;

            var comboExists = await _testCapabilityRepository.ExistsAsync(dto.TestCode, dto.Portfolio);
            if (!comboExists)
                throw new InvalidOperationException("Cannot update, this testcode is not in this portfolio.");
        }

        private async Task ValidateParentProjectAsync(TimeCodeValidDto dto, bool isInsert, TimeCodeValid? existing)
        {
            if (string.IsNullOrEmpty(dto.ParentProject))
                return;

            if (!isInsert && existing!.ParentProject == dto.ParentProject)
                return;

            var projectExists = await _projectRepository.ExistsAsync(dto.ParentProject);
            if (!projectExists)
                throw new InvalidOperationException("Not a valid project");
        }

        private async Task ValidateDuplicateAsync(TimeCodeValidDto dto, bool isInsert)
        {
            if (!isInsert)
                return;

            var duplicate = await _repository.GetTimeCodeValidAsync(dto.WorkGroup, dto.TimeCode, dto.ParentProject);
            if (duplicate != null)
                throw new InvalidOperationException(
                    $"A time code record already exists for WorkGroup '{dto.WorkGroup}', TimeCode '{dto.TimeCode}' and ParentProject '{dto.ParentProject}'.");
        }
    }
}
