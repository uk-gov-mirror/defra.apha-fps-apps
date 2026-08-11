using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class ProgramManagerLinkService : IProgramManagerLinkService
    {
        private readonly IProgramManagerLinkRepository _repository;
        private readonly IMapper _mapper;

        public ProgramManagerLinkService(IProgramManagerLinkRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        
        public async Task<List<ProgramManagerLinkDto>> GetAllProgramManagerLinksAsync()
        {
            List<ProgramManagerLink> entities = await _repository.GetAllProgramManagerLinksAsync();
            return _mapper.Map<List<ProgramManagerLinkDto>>(entities);
        }

        
        public async Task<PaginatedResult<ProgramManagerLinkDto>> GetPagedByManagerAsync(QueryParameters<string> query, string manager)
        {
            if (query is null) throw new ArgumentNullException(nameof(query));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            var parameters = _mapper.Map<Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedByManagerAsync(parameters, manager);
            return _mapper.Map<PaginatedResult<ProgramManagerLinkDto>>(pagedData);
        }

        
        public async Task<List<ProgramManagerLinkDto>> GetByProgramAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program))
                throw new ArgumentException("Program is required.", nameof(program));

            List<ProgramManagerLink> entities = await _repository.GetByProgramAsync(program);
            return _mapper.Map<List<ProgramManagerLinkDto>>(entities);
        }

        public async Task<List<ProgramManagerLinkDto>> GetByManagerAsync(string manager)
        {
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            List<ProgramManagerLink> entities = await _repository.GetByManagerAsync(manager);
            return _mapper.Map<List<ProgramManagerLinkDto>>(entities);
        }

       
        public async Task<ProgramManagerLinkDto?> GetProgramManagerLinkByIdAsync(string program, string manager)
        {
            if (string.IsNullOrWhiteSpace(program))
                throw new ArgumentException("Program is required.", nameof(program));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            ProgramManagerLink? entity = await _repository.GetProgramManagerLinkByIdAsync(program, manager);
            return entity is null ? null : _mapper.Map<ProgramManagerLinkDto>(entity);
        }

        
        public async Task<ProgramManagerLinkDto> CreateProgramManagerLinkAsync(ProgramManagerLinkDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Program))
                throw new ArgumentException("Program is required.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Manager))
                throw new ArgumentException("Manager is required.", nameof(dto));

            bool alreadyExists = await _repository.ProgramManagerLinkExistsAsync(dto.Program, dto.Manager);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"ProgramManagerLink (program='{dto.Program}', manager='{dto.Manager}') already exists.");

            ProgramManagerLink entity = _mapper.Map<ProgramManagerLink>(dto);
            ProgramManagerLink created = await _repository.AddProgramManagerLinkAsync(entity);
            return _mapper.Map<ProgramManagerLinkDto>(created);
        }

       
        public async Task<bool> DeleteProgramManagerLinkAsync(string program, string manager)
        {
            if (string.IsNullOrWhiteSpace(program))
                throw new ArgumentException("Program is required.", nameof(program));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            bool exists = await _repository.ProgramManagerLinkExistsAsync(program, manager);
            if (!exists)
                throw new KeyNotFoundException(
                    $"ProgramManagerLink (program='{program}', manager='{manager}') was not found.");

            return await _repository.DeleteProgramManagerLinkAsync(program, manager);
        }

        public async Task<bool> ProgramManagerLinkExistsAsync(string program, string manager)
        {
            if (string.IsNullOrWhiteSpace(program))
                throw new ArgumentException("Program is required.", nameof(program));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            return await _repository.ProgramManagerLinkExistsAsync(program, manager);
        }

        public async Task<List<ProgramLookupDto>> GetProgramsAsync()
        {
            var entities = await _repository.GetProgramsAsync();
            return _mapper.Map<List<ProgramLookupDto>>(entities);
        }
    }
}
