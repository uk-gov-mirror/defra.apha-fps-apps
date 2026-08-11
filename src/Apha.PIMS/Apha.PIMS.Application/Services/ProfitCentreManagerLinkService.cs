using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class ProfitCentreManagerLinkService : IProfitCentreManagerLinkService
    {
        private readonly IProfitCentreManagerLinkRepository _repository;
        private readonly IMapper _mapper;

        public ProfitCentreManagerLinkService(IProfitCentreManagerLinkRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<ProfitCentreManagerLinkDto>> GetAllProfitCentreManagerLinksAsync()
        {
            List<ProfitCentreManagerLink> entities = await _repository.GetAllProfitCentreManagerLinksAsync();
            return _mapper.Map<List<ProfitCentreManagerLinkDto>>(entities);
        }

        public async Task<PaginatedResult<ProfitCentreManagerLinkDto>> GetPagedByManagerAsync(QueryParameters<string> query, string manager)
        {
            if (query is null) throw new ArgumentNullException(nameof(query));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedByManagerAsync(parameters, manager);
            return _mapper.Map<PaginatedResult<ProfitCentreManagerLinkDto>>(pagedData);
        }

        public async Task<List<ProfitCentreLookupDto>> GetProfitCentresAsync()
        {
            List<ProfitCentreLookup> entities = await _repository.GetProfitCentresAsync();
            return _mapper.Map<List<ProfitCentreLookupDto>>(entities);
        }

        public async Task<List<ProfitCentreManagerLinkDto>> GetByProfitCentreAsync(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                throw new ArgumentException("Profit centre is required.", nameof(profitCentre));

            List<ProfitCentreManagerLink> entities = await _repository.GetByProfitCentreAsync(profitCentre);
            return _mapper.Map<List<ProfitCentreManagerLinkDto>>(entities);
        }

        public async Task<List<ProfitCentreManagerLinkDto>> GetByManagerAsync(string manager)
        {
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            List<ProfitCentreManagerLink> entities = await _repository.GetByManagerAsync(manager);
            return _mapper.Map<List<ProfitCentreManagerLinkDto>>(entities);
        }

        public async Task<ProfitCentreManagerLinkDto?> GetProfitCentreManagerLinkByIdAsync(string profitCentre, string manager)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                throw new ArgumentException("Profit centre is required.", nameof(profitCentre));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            ProfitCentreManagerLink? entity = await _repository.GetProfitCentreManagerLinkByIdAsync(profitCentre, manager);
            return entity is null ? null : _mapper.Map<ProfitCentreManagerLinkDto>(entity);
        }

        public async Task<ProfitCentreManagerLinkDto> CreateProfitCentreManagerLinkAsync(ProfitCentreManagerLinkDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.ProfitCentre))
                throw new ArgumentException("Profit centre is required.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Manager))
                throw new ArgumentException("Manager is required.", nameof(dto));

            bool alreadyExists = await _repository.ProfitCentreManagerLinkExistsAsync(dto.ProfitCentre, dto.Manager);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"ProfitCentreManagerLink (profitcentre='{dto.ProfitCentre}', manager='{dto.Manager}') already exists.");

            ProfitCentreManagerLink entity = _mapper.Map<ProfitCentreManagerLink>(dto);
            ProfitCentreManagerLink created = await _repository.AddProfitCentreManagerLinkAsync(entity);
            return _mapper.Map<ProfitCentreManagerLinkDto>(created);
        }

        public async Task<bool> DeleteProfitCentreManagerLinkAsync(string profitCentre, string manager)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                throw new ArgumentException("Profit centre is required.", nameof(profitCentre));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            bool exists = await _repository.ProfitCentreManagerLinkExistsAsync(profitCentre, manager);
            if (!exists)
                throw new KeyNotFoundException(
                    $"ProfitCentreManagerLink (profitcentre='{profitCentre}', manager='{manager}') was not found.");

            return await _repository.DeleteProfitCentreManagerLinkAsync(profitCentre, manager);
        }

        public async Task<bool> ProfitCentreManagerLinkExistsAsync(string profitCentre, string manager)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                throw new ArgumentException("Profit centre is required.", nameof(profitCentre));
            if (string.IsNullOrWhiteSpace(manager))
                throw new ArgumentException("Manager is required.", nameof(manager));

            return await _repository.ProfitCentreManagerLinkExistsAsync(profitCentre, manager);
        }
    }
}
