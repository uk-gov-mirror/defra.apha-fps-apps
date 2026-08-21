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
    public class MilestoneService : IMilestoneService
    {
        private readonly IMilestoneRepository _repository;
        private readonly IMapper _mapper;

        public MilestoneService(IMilestoneRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<MilestoneDto>> GetAllMilestonesAsync(QueryParameters<string> parameters, string project)
        {
            PaginationParameters<string> paginationParams = _mapper.Map<PaginationParameters<string>>(parameters);
            PagedData<Milestone> pagedData = await _repository.GetAllMilestonesAsync(paginationParams, project);
            List<MilestoneDto> dtos = _mapper.Map<List<MilestoneDto>>(pagedData.Data);
            foreach (MilestoneDto dto in dtos)
                dto.IsLate = dto.DateDue != default && dto.DateCompleted is null && dto.DateDue.Date < DateTime.Today;
            return new PaginatedResult<MilestoneDto>
            {
                Data = dtos,
                PaginationData = _mapper.Map<PaginationDto>(pagedData.PaginationData)
            };
        }       

        public async Task<MilestoneDto?> GetMilestoneAsync(string project, string number)
        {
            Milestone? entity = await _repository.GetMilestoneAsync(project, number);
            if (entity is null) return null;
            MilestoneDto dto = _mapper.Map<MilestoneDto>(entity);
            dto.IsLate = dto.DateDue != default && dto.DateCompleted is null && dto.DateDue.Date < DateTime.Today;
            return dto;
        }

        public async Task<MilestoneDto> SaveMilestoneAsync(MilestoneDto dto, string? changedBy = null)
        {
            var errors = new List<BusinessValidationError>();

            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.Number))
                errors.Add(new BusinessValidationError("Number is required.", "NUMBER_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.IdType))
                errors.Add(new BusinessValidationError("Type is required.", "TYPE_REQUIRED"));
            if (dto.DateDue == default)
                errors.Add(new BusinessValidationError("Date Due is required.", "DATE_DUE_REQUIRED"));
            if (dto.DateCompleted.HasValue && dto.DateCompleted.Value.Date > DateTime.Today)
                errors.Add(new BusinessValidationError("Date completed cannot be after today.", "DATE_COMPLETED_FUTURE"));
            if (dto.OnTarget != 0 && dto.DateDue != default && dto.DateDue.Date < DateTime.Today)
                errors.Add(new BusinessValidationError("A milestone cannot be 'On Target' if the due date has passed.", "ON_TARGET_PAST_DUE"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            Milestone? existing = await _repository.GetMilestoneAsync(dto.Project, dto.Number);
            if (existing is not null)
            {
                errors.Add(new BusinessValidationError("Number already exists.", "NUMBER_EXISTS"));
                throw new BusinessValidationErrorException(errors);
            }

            ApplyMutualExclusions(dto);

            Milestone newEntity = _mapper.Map<Milestone>(dto);
            Milestone created = await _repository.AddMilestoneAsync(newEntity, changedBy);
            return _mapper.Map<MilestoneDto>(created);
        }

        public async Task<MilestoneDto> UpdateMilestoneAsync(MilestoneDto dto, string? changedBy = null)
        {
            var errors = new List<BusinessValidationError>();

            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.Number))
                errors.Add(new BusinessValidationError("Number is required.", "NUMBER_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.IdType))
                errors.Add(new BusinessValidationError("Type is required.", "TYPE_REQUIRED"));
            if (dto.DateDue == default)
                errors.Add(new BusinessValidationError("Date Due is required.", "DATE_DUE_REQUIRED"));
            if (dto.DateCompleted.HasValue && dto.DateCompleted.Value.Date > DateTime.Today)
                errors.Add(new BusinessValidationError("Date completed cannot be after today.", "DATE_COMPLETED_FUTURE"));
            if (dto.OnTarget != 0 && dto.DateDue != default && dto.DateDue.Date < DateTime.Today)
                errors.Add(new BusinessValidationError("A milestone cannot be 'On Target' if the due date has passed.", "ON_TARGET_PAST_DUE"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            Milestone existing = await _repository.GetMilestoneAsync(dto.Project, dto.Number)
                ?? throw new BusinessValidationErrorException(
                    [new BusinessValidationError("Milestone not found.", "NOT_FOUND")]);

            ApplyMutualExclusions(dto);

            _mapper.Map(dto, existing);
            Milestone updated = await _repository.UpdateMilestoneAsync(existing, changedBy);
            return _mapper.Map<MilestoneDto>(updated);
        }

        public async Task<MilestoneDto> UpdateMilestoneAsync_PMD(string project, string number, short underReview, short onTarget, DateTime? dateCompleted, string? projectLeaderComment, string? changedBy = null)
        {
            var errors = new List<BusinessValidationError>();

            if (string.IsNullOrWhiteSpace(project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));
            if (string.IsNullOrWhiteSpace(number))
                errors.Add(new BusinessValidationError("Number is required.", "NUMBER_REQUIRED"));
            if (dateCompleted.HasValue && dateCompleted.Value.Date > DateTime.Today)
                errors.Add(new BusinessValidationError("Completion Date cannot be in the future.", "DATE_COMPLETED_FUTURE"));

            Milestone? existing = await _repository.GetMilestoneAsync(project, number);
            if (existing is null)
                errors.Add(new BusinessValidationError("Milestone not found.", "NOT_FOUND"));
            else
            {
                DateTime dueDate = existing.DateDue;
                if (dueDate != default && dueDate < DateTime.Now && onTarget != 0)
                    errors.Add(new BusinessValidationError("Milestone cannot be On Target as the due date has passed.", "ON_TARGET_PAST_DUE"));

                if (onTarget != 0 && underReview != 0)
                    errors.Add(new BusinessValidationError("Milestone cannot be On Target and Under Review.", "ON_TARGET_AND_UNDER_REVIEW"));

                if (dateCompleted.HasValue && underReview != 0)
                    errors.Add(new BusinessValidationError("Milestone cannot be completed and Under Review.", "COMPLETED_AND_UNDER_REVIEW"));
            }

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            // Create a DTO for mutual exclusion logic
            var dto = new MilestoneDto
            {
                UnderSdReview = underReview,
                OnTarget = onTarget,
                DateCompleted = dateCompleted,
                ProjectLeaderComment = projectLeaderComment
            };

           

            // Update only PMD-specific fields
            Milestone updated = await _repository.UpdateMilestoneAsync_PMD(
                project, number,
                dto.UnderSdReview,
                dto.OnTarget ?? 0,
                dto.DateCompleted,
                dto.ProjectLeaderComment,
                changedBy);

            return _mapper.Map<MilestoneDto>(updated);
        }

        private static void ApplyMutualExclusions(MilestoneDto dto)
        {
            if (dto.DateCompleted.HasValue)
            {
                dto.UnderSdReview = 0;
                dto.OnTarget = 0;
            }
            if (dto.OnTarget != 0)
            {
                dto.UnderSdReview = 0;
                dto.DateCompleted = null;
            }
            if (dto.UnderSdReview != 0)
            {
                dto.OnTarget = 0;
                dto.DateCompleted = null;
            }
        }

        public async Task<bool> DeleteMilestoneAsync(string project, string number)
            => await _repository.DeleteMilestoneAsync(project, number);

        public async Task<bool> UpdateFormRequiredAsync(string parentproject, bool formRequired)
            => await _repository.UpdateFormRequiredAsync(parentproject, formRequired);

        public async Task<List<MilestoneTypeDto>> GetMilestoneTypesAsync(string? milestoneDeliverable = null)
        {
            List<MilestoneType> types = await _repository.GetMilestoneTypesAsync(milestoneDeliverable);
            return _mapper.Map<List<MilestoneTypeDto>>(types);
        }
        public async Task<PaginatedResult<MilestoneFormDatesDto>> GetAllMilestoneFormDatesAsync(QueryParameters<string> parameters, string parentProject)
        {
            PaginationParameters<string> paginationParams = _mapper.Map<PaginationParameters<string>>(parameters);
            PagedData<MilestoneFormDates> pagedData = await _repository.GetAllMilestoneFormDatesAsync(paginationParams, parentProject);
            return new PaginatedResult<MilestoneFormDatesDto>
            {
                Data = _mapper.Map<List<MilestoneFormDatesDto>>(pagedData.Data),
                PaginationData = _mapper.Map<PaginationDto>(pagedData.PaginationData)
            };
        }

        public async Task<MilestoneFormDatesDto?> GetMilestoneFormDatesAsync(short year, string parentProject)
        {
            MilestoneFormDates? entity = await _repository.GetMilestoneFormDatesAsync(year, parentProject);
            return entity is null ? null : _mapper.Map<MilestoneFormDatesDto>(entity);
        }

        public async Task<MilestoneFormDatesDto> SaveMilestoneFormDatesAsync(MilestoneFormDatesDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.ParentProject))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));
            if (dto.Year == 0)
                errors.Add(new BusinessValidationError("Financial Year is required.", "YEAR_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            MilestoneFormDates? existing = await _repository.GetMilestoneFormDatesAsync(dto.Year, dto.ParentProject);
            if (existing is null)
            {
                MilestoneFormDates newEntity = _mapper.Map<MilestoneFormDates>(dto);
                MilestoneFormDates created = await _repository.AddMilestoneFormDatesAsync(newEntity);
                return _mapper.Map<MilestoneFormDatesDto>(created);
            }

            _mapper.Map(dto, existing);
            MilestoneFormDates updated = await _repository.UpdateMilestoneFormDatesAsync(existing);
            return _mapper.Map<MilestoneFormDatesDto>(updated);
        }

        public async Task<bool> DeleteMilestoneFormDatesAsync(short year, string parentProject)
            => await _repository.DeleteMilestoneFormDatesAsync(year, parentProject);

        public async Task<PaginatedResult<LogMilestoneDto>> GetLogMilestonesAsync(QueryParameters<string> parameters,string? project,string? numberPart1,string? numberPart2)
        {
            PaginationParameters<string> paginationParams = _mapper.Map<PaginationParameters<string>>(parameters);
            PagedData<LogMilestone> pagedData = await _repository.GetLogMilestonesAsync(paginationParams, project, numberPart1, numberPart2);
            return new PaginatedResult<LogMilestoneDto>
            {
                Data = _mapper.Map<List<LogMilestoneDto>>(pagedData.Data),
                PaginationData = _mapper.Map<PaginationDto>(pagedData.PaginationData)
            };
        }
        // ── Staging / Import ─────────────────────────────────────────────────
        public async Task<PaginatedResult<StagingMilestoneDto>> GetAllStagingRowsAsync(QueryParameters<string> parameters, string? createdBy = null)
        {

            PaginationParameters<string> paginationParams = _mapper.Map<PaginationParameters<string>>(parameters);
            PagedData<StagingMilestone> pagedData = await _repository.GetAllStagingRowsAsync(paginationParams, createdBy);
            return new PaginatedResult<StagingMilestoneDto>
            {
                Data = _mapper.Map<List<StagingMilestoneDto>>(pagedData.Data),
                PaginationData = _mapper.Map<PaginationDto>(pagedData.PaginationData)
            };

        }

        public async Task<List<StagingMilestoneDto>> GetStagingRowsAsync(int id)
        {
            List<StagingMilestone> entities = await _repository.GetStagingRowsAsync(id);
            return _mapper.Map<List<StagingMilestoneDto>>(entities);
        }

        public async Task<StagingMilestoneDto> AddStagingRowAsync(StagingMilestoneDto dto, int year, string? createdBy = null)
        {
            string? program = null;
            if (!string.IsNullOrWhiteSpace(dto.Project))
            {
                program = await _repository.GetProgramByProjectAsync(dto.Project);
            }

            bool isSurvProgram = program?.EndsWith("surv", StringComparison.OrdinalIgnoreCase) == true;
            if (string.IsNullOrWhiteSpace(dto.Number) && !string.IsNullOrWhiteSpace(dto.Project) && isSurvProgram)
                dto.Number = await _repository.GetNextMilestoneNumberAsync(dto.Project, year);


            var errors = new List<BusinessValidationError>();
            if (dto.DateDue == default)
                errors.Add(new BusinessValidationError("Date Due is required.", "DATE_DUE_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.Number))
                errors.Add(new BusinessValidationError("Number is required.", "NUMBER_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.Description))
                errors.Add(new BusinessValidationError("Description is required.", "DESCRIPTION_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            StagingMilestone entity = _mapper.Map<StagingMilestone>(dto);
            entity.DateDue = DateTime.SpecifyKind(entity.DateDue, DateTimeKind.Unspecified);
            StagingMilestone created = await _repository.AddStagingRowAsync(entity, createdBy);
            return _mapper.Map<StagingMilestoneDto>(created);
        }

        public async Task<StagingMilestoneDto> UpdateStagingRowAsync(StagingMilestoneDto dto, string? createdBy = null)
        {
            string? program = null;
            if (!string.IsNullOrWhiteSpace(dto.Project))
                program = await _repository.GetProgramByProjectAsync(dto.Project);

            bool isSurvProgram = program?.EndsWith("surv", StringComparison.OrdinalIgnoreCase) == true;
            if (string.IsNullOrWhiteSpace(dto.Number) && !string.IsNullOrWhiteSpace(dto.Project) && isSurvProgram)
            {
                int year = dto.DateDue == default ? DateTime.Today.Year : dto.DateDue.Year;
                dto.Number = await _repository.GetNextMilestoneNumberAsync(dto.Project, year);
            }

            var errors = new List<BusinessValidationError>();
            if (dto.Id == 0)
                errors.Add(new BusinessValidationError("Id is required.", "ID_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.Number))
                errors.Add(new BusinessValidationError("Number is required.", "NUMBER_REQUIRED"));
            if (dto.DateDue == default)
                errors.Add(new BusinessValidationError("Date Due is required.", "DATE_DUE_REQUIRED"));
            if (string.IsNullOrWhiteSpace(dto.Description))
                errors.Add(new BusinessValidationError("Description is required.", "DESCRIPTION_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            StagingMilestone entity = _mapper.Map<StagingMilestone>(dto);
            entity.DateDue = DateTime.SpecifyKind(entity.DateDue, DateTimeKind.Unspecified);
            entity.Note = null;
            StagingMilestone updated = await _repository.UpdateStagingRowAsync(entity, createdBy);
            return _mapper.Map<StagingMilestoneDto>(updated);
        }

        public async Task<bool> DeleteStagingRowAsync(int id, string? createdBy = null)
            => await _repository.DeleteStagingRowAsync(id, createdBy);

        public async Task<int> ClearStagingAsync(string project, string? createdBy = null)
            => await _repository.ClearStagingAsync(project, createdBy);

        public async Task ValidateStagingAsync(string project, string? typeId, bool isDeliverableMode, string? createdBy = null)
            => await _repository.ValidateStagingAsync(project, typeId, isDeliverableMode, createdBy);

        public async Task<int> ImportStagingAsync(string project, string? changedBy = null, string? createdBy = null)
            => await _repository.ImportStagingAsync(project, changedBy, createdBy);

        public async Task<int> ImportWithOverwriteAsync(string project, string? changedBy = null, string? createdBy = null)
            => await _repository.ImportWithOverwriteAsync(project, changedBy, createdBy);

        public async Task<string> GetNextMilestoneNumberAsync(string project, int year)
            => await _repository.GetNextMilestoneNumberAsync(project, year);

        public async Task<List<ProjectYearManagerDto>> GetProjectYearManagersAsync(int year, string? loginEmail = null, bool viewSpecificProject = false)
        {
            List<ProjectYearManager> entities = await _repository.GetProjectYearManagersAsync(year, loginEmail, viewSpecificProject);
            return _mapper.Map<List<ProjectYearManagerDto>>(entities);
        }
        public async Task<PaginatedResult<MilestoneDto>> GetPMDMilestonesAsync(QueryParameters<string> parameters, string project)
        {
            PaginationParameters<string> paginationParams = _mapper.Map<PaginationParameters<string>>(parameters);
            PagedData<Milestone> pagedData = await _repository.GetPMDMilestonesAsync(paginationParams, project);
            List<MilestoneDto> dtos = _mapper.Map<List<MilestoneDto>>(pagedData.Data);
            foreach (MilestoneDto dto in dtos)
                dto.IsLate = dto.DateDue != default && dto.DateCompleted is null && dto.DateDue.Date < DateTime.Today;

            return new PaginatedResult<MilestoneDto>
            {
                Data = dtos,
                PaginationData = _mapper.Map<PaginationDto>(pagedData.PaginationData)
            };
        }
    }
}
