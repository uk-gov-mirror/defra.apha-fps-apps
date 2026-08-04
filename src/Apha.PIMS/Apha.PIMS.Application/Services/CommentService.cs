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
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repository;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        
        public async Task<PaginatedResult<CommentDto>> GetCommentsByProjectAsync(string project, int? year, QueryParameters<string> query, string? topic = null)
        {
            PaginationParameters<string> filter = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<Comment> result = await _repository.GetCommentsByProjectAsync(project, year, filter, topic);
            return _mapper.Map<PaginatedResult<CommentDto>>(result);
        }

        public async Task<CommentDto?> GetByIdAsync(int CommentNo)
        {
            Comment? entity = await _repository.GetByIdAsync(CommentNo);
            return entity is null ? null : _mapper.Map<CommentDto>(entity);
        }



        public async Task<CommentDto> AddAsync(CommentDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));

            if (dto.Year is null or 0)
                errors.Add(new BusinessValidationError("Year is required.", "YEAR_REQUIRED"));

            if (string.IsNullOrWhiteSpace(dto.Topic))
                errors.Add(new BusinessValidationError("Topic is required.", "TOPIC_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            
            bool duplicate = await _repository.ExistsAsync(dto.Project!, (short)dto.Year!.Value, dto.Topic!);
            if (duplicate)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"A comment for project '{dto.Project}', year '{dto.Year}', topic '{dto.Topic}' already exists.",
                        "COMMENT_DUPLICATE")
                ]);

            Comment entity = _mapper.Map<Comment>(dto);
            entity.DateEntered = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            Comment created = await _repository.AddAsync(entity);
            return _mapper.Map<CommentDto>(created);
        }

        public async Task<CommentDto> UpdateAsync(CommentDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));

            if (dto.Year is null or 0)
                errors.Add(new BusinessValidationError("Year is required.", "YEAR_REQUIRED"));

            if (string.IsNullOrWhiteSpace(dto.Topic))
                errors.Add(new BusinessValidationError("Topic is required.", "TOPIC_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            Comment existing = await _repository.GetByIdAsync(dto.CommentNo)
                ?? throw new KeyNotFoundException($"Comment {dto.CommentNo} not found.");

            existing.Project = dto.Project!;
            existing.Year = (short)dto.Year!.Value;
            existing.Topic = dto.Topic!;
            existing.CommentText = dto.CommentText;
            existing.MadeBy = dto.MadeBy;
            Comment updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<CommentDto>(updated);
        }

        public async Task<bool> DeleteAsync(int CommentNo)
        {
            return await _repository.DeleteAsync(CommentNo);
        }

        public async Task<IEnumerable<CommentTopicDto>> GetCommentTopicsAsync()
        {
            var topics = await _repository.GetCommentTopicsAsync();
            return _mapper.Map<IEnumerable<CommentTopicDto>>(topics);
        }

        public async Task<double?> GetForecastSpendByProjectAsync(string project)
        {
            return await _repository.GetForecastSpendByProjectAsync(project);
        }

        public async Task<double?> UpdateForecastSpendByProjectAsync(string project, double? forecastSpend)
        {
            return await _repository.UpdateForecastSpendByProjectAsync(project, forecastSpend);
        }
    }
}
