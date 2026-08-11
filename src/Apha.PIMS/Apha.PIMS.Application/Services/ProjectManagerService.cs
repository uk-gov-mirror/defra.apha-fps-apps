using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class ProjectManagerService : IProjectManagerService
    {
        private readonly IProjectManagerRepository _repository;
        private readonly IMapper _mapper;

        public ProjectManagerService(IProjectManagerRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<ProjectManagerDto>> GetAllProjectManagersAsync()
        {
            List<ProjectManager> entities = await _repository.GetAllProjectManagersAsync();
            return _mapper.Map<List<ProjectManagerDto>>(entities);
        }

        public async Task<PaginatedResult<ProjectManagerDto>> GetPagedProjectManagersAsync(QueryParameters<string>? query = null)
        {
            query ??= new QueryParameters<string>();

            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedProjectManagersAsync(parameters);
            return _mapper.Map<PaginatedResult<ProjectManagerDto>>(pagedData);
        }

        public async Task<List<string>> GetManagerNamesAsync()
        {
            return await _repository.GetManagerNamesAsync();
        }

        public async Task<ProjectManagerDto?> GetProjectManagerByNameAsync(string projectManagerName)
        {
            if (string.IsNullOrWhiteSpace(projectManagerName))
                throw new ArgumentException("Project manager name is required.", nameof(projectManagerName));

            ProjectManager? entity = await _repository.GetProjectManagerByNameAsync(projectManagerName);
            return entity is null ? null : _mapper.Map<ProjectManagerDto>(entity);
        }

        public async Task<ProjectManagerDto> CreateProjectManagerAsync(ProjectManagerDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.ProjectManager))
                throw new ArgumentException("Project manager name is required.", nameof(dto));

            bool alreadyExists = await _repository.ProjectManagerExistsAsync(dto.ProjectManager);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"ProjectManager '{dto.ProjectManager}' already exists.");

            ProjectManager entity = _mapper.Map<ProjectManager>(dto);
            ProjectManager created = await _repository.AddProjectManagerAsync(entity);
            return _mapper.Map<ProjectManagerDto>(created);
        }

        public async Task<ProjectManagerDto> UpdateProjectManagerAsync(ProjectManagerDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.ProjectManager))
                throw new ArgumentException("Project manager name is required.", nameof(dto));

            bool exists = await _repository.ProjectManagerExistsAsync(dto.ProjectManager);
            if (!exists)
                throw new KeyNotFoundException($"ProjectManager '{dto.ProjectManager}' was not found.");

            ProjectManager entity = _mapper.Map<ProjectManager>(dto);
            ProjectManager updated = await _repository.UpdateProjectManagerAsync(entity);
            return _mapper.Map<ProjectManagerDto>(updated);
        }

        public async Task<bool> DeleteProjectManagerAsync(string projectManagerName)
        {
            if (string.IsNullOrWhiteSpace(projectManagerName))
                throw new ArgumentException("Project manager name is required.", nameof(projectManagerName));

            bool exists = await _repository.ProjectManagerExistsAsync(projectManagerName);
            if (!exists)
                throw new KeyNotFoundException($"ProjectManager '{projectManagerName}' was not found.");

            var deleted = await _repository.DeleteProjectManagerAsync(projectManagerName);
            if (!deleted)
                throw new KeyNotFoundException($"ProjectManager '{projectManagerName}' was not found.");

            return true;
        }

        public async Task<bool> ProjectManagerExistsAsync(string projectManagerName)
        {
            if (string.IsNullOrWhiteSpace(projectManagerName))
                throw new ArgumentException("Project manager name is required.", nameof(projectManagerName));

            return await _repository.ProjectManagerExistsAsync(projectManagerName);
        }
    }
}
