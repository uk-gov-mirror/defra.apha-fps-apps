using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using MapsterMapper;

namespace Apha.PIMS.Application.Services
{
    public class ProjectDetailsService : IProjectDetailsService
    {
        private readonly IProjectDetailsRepository _repository;
        private readonly IMapper _mapper;

        public ProjectDetailsService(IProjectDetailsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ProjectDetailDto?> GetPimsDetailAsync(string parentproject)
        {
            ProjectDetail? entity = await _repository.GetPimsDetailAsync(parentproject);
            return entity is null ? null : _mapper.Map<ProjectDetailDto>(entity);
        }

        public async Task<ProjectDetailDto> SavePimsDetailAsync(ProjectDetailDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Parentproject))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectDetail? existing = await _repository.GetPimsDetailAsync(dto.Parentproject!);
            if (existing is null)
            {                
                ProjectDetail newEntity = _mapper.Map<ProjectDetail>(dto);
                ProjectDetail created = await _repository.AddPimsDetailAsync(newEntity);
                return _mapper.Map<ProjectDetailDto>(created);
            }

            _mapper.Map(dto, existing);
            ProjectDetail updated = await _repository.UpdatePimsDetailAsync(existing);
            return _mapper.Map<ProjectDetailDto>(updated);
        }

        public async Task<ProposedProjectDto?> GetProposedProjectAsync(string parentproject)
        {
            ProposedProject? entity = await _repository.GetProposedProjectAsync(parentproject);
            return entity is null ? null : _mapper.Map<ProposedProjectDto>(entity);
        }

        public async Task<ProposedProjectDto> UpdateProposedProjectAsync(ProposedProjectDto dto, string transferTo)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Parentproject))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));           

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProposedProject entity = _mapper.Map<ProposedProject>(dto);
            ProposedProject updated = await _repository.UpdateProposedProjectAsync(entity, transferTo);
            return _mapper.Map<ProposedProjectDto>(updated);
        }

        public async Task<List<RiskDto>> GetAllRiskAsync()
        {
            List<Risk> entities = await _repository.GetAllRiskAsync();
            return _mapper.Map<List<RiskDto>>(entities);
        }

        public async Task<List<YearDto>> GetAllYearAsync()
        {
            List<Year> entities = await _repository.GetAllYearAsync();

            int currentYear = DateTime.Now.Year;
            if (DateTime.Now.Month < 4)
            {
                currentYear--;
            }

            if (!entities.Any(e => e.Value == currentYear))
            {
                entities.Add(new Year { Value = currentYear, Latestmonthreleased = null });
            }

            return _mapper.Map<List<YearDto>>(entities);
        }

        public async Task<ProjectDto?> GetFpsProjectByIdAsync(string parentproject)
        {
            Project? entity = await _repository.GetFpsProjectByIdAsync(parentproject);
            return entity is null ? null : _mapper.Map<ProjectDto>(entity);
        }
    }
}
