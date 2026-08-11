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
    public class PublicationTypeService : IPublicationTypeService
    {
        private readonly IPublicationTypeRepository _repository;
        private readonly IMapper _mapper;

        public PublicationTypeService(IPublicationTypeRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<PublicationTypeDto>> GetAllPublicationTypesAsync()
        {
            List<PublicationType> entities = await _repository.GetAllPublicationTypesAsync();
            return _mapper.Map<List<PublicationTypeDto>>(entities);
        }

        public async Task<PaginatedResult<PublicationTypeDto>> GetPagedPublicationTypesAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedPublicationTypesAsync(parameters);
            return _mapper.Map<PaginatedResult<PublicationTypeDto>>(pagedData);
        }

        public async Task<PublicationTypeDto?> GetPublicationTypeByCodeAsync(string type)
        {
            PublicationType? entity = await _repository.GetPublicationTypeByCodeAsync(type);
            return entity is null ? null : _mapper.Map<PublicationTypeDto>(entity);
        }

        public async Task<PublicationTypeDto> CreatePublicationTypeAsync(PublicationTypeDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool exists = await _repository.PublicationTypeExistsAsync(dto.Type);
            if (exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Type code '{dto.Type}' already exists.",
                        "PUBLICATION_TYPE_ALREADY_EXISTS")
                };
                throw new BusinessValidationErrorException(errors);
            }

            PublicationType entity = _mapper.Map<PublicationType>(dto);
            PublicationType created = await _repository.AddPublicationTypeAsync(entity);
            return _mapper.Map<PublicationTypeDto>(created);
        }

        public async Task<PublicationTypeDto> UpdatePublicationTypeAsync(PublicationTypeDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool exists = await _repository.PublicationTypeExistsAsync(dto.Type);
            if (!exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Publication type with code '{dto.Type}' was not found.",
                        "PUBLICATION_TYPE_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            PublicationType entity = _mapper.Map<PublicationType>(dto);
            PublicationType updated = await _repository.UpdatePublicationTypeAsync(entity);
            return _mapper.Map<PublicationTypeDto>(updated);
        }

        public async Task<bool> DeletePublicationTypeAsync(string type)
        {
            bool exists = await _repository.PublicationTypeExistsAsync(type);
            if (!exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Publication type with code '{type}' was not found.",
                        "PUBLICATION_TYPE_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            return await _repository.DeletePublicationTypeAsync(type);
        }

        public async Task<bool> PublicationTypeExistsAsync(string type)
        {
            return await _repository.PublicationTypeExistsAsync(type);
        }
    }
}
