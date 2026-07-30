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
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public ProjectService(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllProjectsAsync();
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsForAllUsersAsync()
        {
            var projects = await _projectRepository.GetAllProjectsForAllUsersAsync();
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }

        public async Task<IEnumerable<ProjectDto>> GetAllPactProjectsAsync()
        {
            var projects = await _projectRepository.GetAllPactProjectsAsync();
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }

        public async Task<PaginatedResult<ProjectDto>> GetPagedProjectsAsync(QueryParameters<string> query)
        {
            var pagedProjects = await _projectRepository.GetPagedProjectsAsync(
                _mapper.Map<PaginationParameters<string>>(query));
            return _mapper.Map<PaginatedResult<ProjectDto>>(pagedProjects);
        }

        public async Task<PaginatedResult<ProjectDto>> GetPagedProjectsByUserAsync(QueryParameters<string> query)
        {
            var pagedProjects = await _projectRepository.GetPagedProjectsByUserAsync(
                _mapper.Map<PaginationParameters<string>>(query));
            return _mapper.Map<PaginatedResult<ProjectDto>>(pagedProjects);
        }

        public async Task<PaginatedResult<ProjectDto>> GetPagedPactProjectsAsync(QueryParameters<string> query)
        {
            var pagedProjects = await _projectRepository.GetPagedPactProjectsAsync(
                _mapper.Map<PaginationParameters<string>>(query));
            return _mapper.Map<PaginatedResult<ProjectDto>>(pagedProjects);
        }

        public async Task<PaginatedResult<ProjectDto>> GetPagedPactProjectsByProgramAsync(QueryParameters<string> query, string programNo)
        {
            var pagedProjects = await _projectRepository.GetPagedPactProjectsByProgramAsync(
                _mapper.Map<PaginationParameters<string>>(query), programNo);
            return _mapper.Map<PaginatedResult<ProjectDto>>(pagedProjects);
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(string parentProject)
        {
            var project = await _projectRepository.GetProjectByIdAsync(parentProject);
            return project == null ? null : _mapper.Map<ProjectDto>(project);
        }

        public async Task<ProjectDto> CreateProjectAsync(ProjectDto projectDto)
        {
            // Derived from tI_tlkpProject: validates Program FK exists in tlkpProgram
            if (!string.IsNullOrWhiteSpace(projectDto.Program) &&
                !await _projectRepository.CheckProgramExistsAsync(projectDto.Program))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"Cannot create project: Program '{projectDto.Program}' does not exist.",
                        "PROGRAM_NOT_FOUND")
                ]);
            }

            var project = _mapper.Map<Project>(projectDto);
            var created = await _projectRepository.CreateProjectAsync(project);
            return _mapper.Map<ProjectDto>(created);
        }

        public async Task<ProjectDto> UpdateProjectAsync(ProjectDto projectDto)
        {
            // Derived from tU_tlkpProject: validates Program FK exists in tlkpProgram
            if (!string.IsNullOrWhiteSpace(projectDto.Program) &&
                !await _projectRepository.CheckProgramExistsAsync(projectDto.Program))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"Cannot update project: Program '{projectDto.Program}' does not exist.",
                        "PROGRAM_NOT_FOUND")
                ]);
            }

            var project = _mapper.Map<Project>(projectDto);
            var updated = await _projectRepository.UpdateProjectAsync(project);
            return _mapper.Map<ProjectDto>(updated);
        }

        public async Task<ProjectDto?> UpdatePactProjectDetailsAsync(ProjectDto projectDto)
        {
            // Derived from tU_tlkpProject: validates Program FK exists in tlkpProgram
            if (!string.IsNullOrWhiteSpace(projectDto.Program) &&
                !await _projectRepository.CheckProgramExistsAsync(projectDto.Program))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"Cannot update project: Program '{projectDto.Program}' does not exist.",
                        "PROGRAM_NOT_FOUND")
                ]);
            }

            var project = _mapper.Map<Project>(projectDto);
            var updated = await _projectRepository.UpdatePactProjectDetailsAsync(project);
            return updated == null ? null : _mapper.Map<ProjectDto>(updated);
        }

        public async Task<ProjectDto?> UpdatePactPortfolioDetailsAsync(ProjectDto projectDto)
        {
            var project = _mapper.Map<Project>(projectDto);
            var updated = await _projectRepository.UpdatePactPortfolioDetailsAsync(project);
            return updated == null ? null : _mapper.Map<ProjectDto>(updated);
        }

        public async Task<ProjectDto?> UpdateFpsPortfolioDetailsAsync(ProjectDto projectDto)
        {
            if (!string.IsNullOrWhiteSpace(projectDto.Program) &&
                !await _projectRepository.CheckProgramExistsAsync(projectDto.Program))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"Cannot update portfolio: Program '{projectDto.Program}' does not exist.",
                        "PROGRAM_NOT_FOUND")
                ]);
            }

            var project = _mapper.Map<Project>(projectDto);
            var updated = await _projectRepository.UpdateFpsPortfolioDetailsAsync(project);
            return updated == null ? null : _mapper.Map<ProjectDto>(updated);
        }

        public async Task<bool> DeleteProjectAsync(string parentProject)
        {
            var hasAssociations = await _projectRepository.HasAssociatedJobCodesAsync(parentProject);
            if (hasAssociations)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"Project '{parentProject}' is associated with existing job codes and cannot be deleted.",
                        "PROJECT_HAS_ASSOCIATIONS")
                ]);
            }

            return await _projectRepository.DeleteProjectAsync(parentProject);
        }

        public async Task<PaginatedResult<ProjectDto>> GetProjectsByProgramAsync(QueryParameters<string> query, string programNo)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var projects = await _projectRepository.GetProjectsByProgramAsync(filter, programNo);
            return _mapper.Map<PaginatedResult<ProjectDto>>(projects);
        }

        public async Task<PaginatedResult<ProjectDto>> GetProjectsByProjectGroupAsync(QueryParameters<string> query, string projectGroup)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var projects = await _projectRepository.GetProjectsByProjectGroupAsync(filter, projectGroup);
            return _mapper.Map<PaginatedResult<ProjectDto>>(projects);
        }

        public async Task<PaginatedResult<ProjectDto>> GetProjectsByProgramProjectProfitabilityVLAAsync(QueryParameters<string> query, string programNo)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var projects = await _projectRepository.GetProjectsByProgramProjectProfitabilityVLAAsync(filter, programNo);
            return _mapper.Map<PaginatedResult<ProjectDto>>(projects);
        }

        public async Task<PaginatedResult<ProjectDto>> GetProjectsByProjectGroupProjectProfitabilityVLAAsync(QueryParameters<string> query, string projectGroup)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var projects = await _projectRepository.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(filter, projectGroup);
            return _mapper.Map<PaginatedResult<ProjectDto>>(projects);
        }

        public async Task<bool> CheckProjectExistsAsync(string newProject)
        {
            ArgumentNullException.ThrowIfNull(newProject);
            return await _projectRepository.CheckProjectExistsAsync(newProject);
        }

        public async Task<bool> CheckProjectExistsInFarmFileAsync(string oldProject)
        {
            ArgumentNullException.ThrowIfNull(oldProject);
            return await _projectRepository.CheckProjectExistsInFarmFileAsync(oldProject);
        }

        public async Task ChangeProjectCodeAsync(string oldCode, string newCode)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(oldCode))
                errors.Add(new BusinessValidationError("Old project code is required.", "OLD_CODE_REQUIRED"));
            if (string.IsNullOrWhiteSpace(newCode))
                errors.Add(new BusinessValidationError("New project code cannot be empty.", "NEW_CODE_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            bool oldCodeExists = await _projectRepository.CheckProjectExistsAsync(oldCode);
            if (!oldCodeExists)
                errors.Add(new BusinessValidationError($"Project '{oldCode}' not found.", "OLD_CODE_NOT_FOUND"));

            bool newCodeExists = await _projectRepository.CheckProjectExistsAsync(newCode);
            if (newCodeExists)
                errors.Add(new BusinessValidationError("This code is already in use.", "CODE_ALREADY_EXISTS"));

            bool farmFileDataExists = await _projectRepository.CheckProjectExistsInFarmFileAsync(oldCode);
            if (farmFileDataExists)
                errors.Add(new BusinessValidationError("Cannot change code, data exists in Farm File for old code.", "FARM_FILE_DATA_EXISTS"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            await _projectRepository.ChangeProjectCodeAsync(oldCode, newCode);
        }

        public async Task DeleteProjectAndChildrenAsync(string parentProject)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(parentProject))
            {
                errors.Add(new BusinessValidationError("Parent project code is required.", "PARENT_PROJECT_REQUIRED"));
                throw new BusinessValidationErrorException(errors);
            }

            if (await _projectRepository.HasPlannedTestsAsync(parentProject))
                errors.Add(new BusinessValidationError("Cannot delete project, it still has tests planned.", "HAS_PLANNED_TESTS"));

            if (await _projectRepository.HasMonthlyOutputAsync(parentProject))
                errors.Add(new BusinessValidationError("Cannot delete project, there are Monthly Tests records.", "HAS_MONTHLY_OUTPUT"));

            if (await _projectRepository.HasMonthlyTimeAsync(parentProject))
                errors.Add(new BusinessValidationError("Cannot delete project, there are Monthly Time records.", "HAS_MONTHLY_TIME"));

            if (await _projectRepository.HasProjectInvoicesAsync(parentProject))
                errors.Add(new BusinessValidationError("Cannot delete project, there are Invoice records.", "HAS_INVOICES"));

            if (await _projectRepository.HasProjectSubcontractsAsync(parentProject))
                errors.Add(new BusinessValidationError("Cannot delete project, there are Subcontract records.", "HAS_SUBCONTRACTS"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            await _projectRepository.DeleteProjectAndChildrenAsync(parentProject);
        }

        public async Task<PaginatedResult<ProjectProfitabilityDto>> GetProjectProfitabilityAsync(
            QueryParameters<string> query, string programNo, string workTypeFilter)
        {
            var pagedResult = await _projectRepository.GetProjectProfitabilityAsync(
                _mapper.Map<PaginationParameters<string>>(query), programNo, workTypeFilter);
            return _mapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedResult);
        }

        public async Task<PaginatedResult<ProjectProfitabilityDto>> GetProjectGroupProfitabilityAsync(
            QueryParameters<string> query, string projectGroup, string workTypeFilter)
        {
            var pagedResult = await _projectRepository.GetProjectGroupProfitabilityAsync(
                _mapper.Map<PaginationParameters<string>>(query), projectGroup, workTypeFilter);
            return _mapper.Map<PaginatedResult<ProjectProfitabilityDto>>(pagedResult);
        }

        public async Task<PaginatedResult<ProjectProfitabilityVlaDto>> GetProjectProfitabilityVlaAsync(
            QueryParameters<string> query, string? projectStatus = null, string? programNo = null, string? manager = null, string? customer = null)
        {
            ArgumentNullException.ThrowIfNull(query);
            var pagedResult = await _projectRepository.GetProjectProfitabilityVlaAsync(
                _mapper.Map<PaginationParameters<string>>(query), projectStatus, programNo, manager, customer);
            return _mapper.Map<PaginatedResult<ProjectProfitabilityVlaDto>>(pagedResult);
        }

        public async Task<PaginatedResult<ProjectStaffReplanDto>> GetProjectStaffReplanAsync(QueryParameters<string> query, string workgroup)
        {
            var pagedResult = await _projectRepository.GetProjectStaffReplanAsync(
                _mapper.Map<PaginationParameters<string>>(query), workgroup);
            return _mapper.Map<PaginatedResult<ProjectStaffReplanDto>>(pagedResult);
        }
    }
}
