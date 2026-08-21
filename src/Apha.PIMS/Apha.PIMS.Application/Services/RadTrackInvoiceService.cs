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
    
    public class RadTrackInvoiceService : IRadTrackInvoiceService
    {
        private readonly IRadTrackInvoiceRepository _repository;
        private readonly IMapper _mapper;

        public RadTrackInvoiceService(IRadTrackInvoiceRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<RadTrackInvoiceDto>> GetAllAsync(QueryParameters<RadTrackInvoiceFilter> parameters)
        {
            if (parameters is null)
                throw new ArgumentException("Query parameters must not be null.", nameof(parameters));

            PaginationParameters<RadTrackInvoiceFilter> paginationParams =
                _mapper.Map<PaginationParameters<RadTrackInvoiceFilter>>(parameters);

            PagedData<RadTrackInvoice> pagedData = await _repository.GetAllAsync(paginationParams);

            return new PaginatedResult<RadTrackInvoiceDto>
            {
                Data = _mapper.Map<List<RadTrackInvoiceDto>>(pagedData.Data),
                PaginationData = _mapper.Map<PaginationDto>(pagedData.PaginationData)
            };
        }

        public async Task<RadTrackInvoiceDto?> GetByIdAsync(int invoiceCounter)
        {
            RadTrackInvoice? entity = await _repository.GetByIdAsync(invoiceCounter);
            return entity is null ? null : _mapper.Map<RadTrackInvoiceDto>(entity);
        }

       
        public async Task<RadTrackInvoiceDto> CreateAsync(RadTrackInvoiceDto dto)
        {
            if (dto is null)
                throw new ArgumentException("Invoice DTO must not be null.", nameof(dto));

            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));         
                       
            if (!dto.DueDate.HasValue)
                errors.Add(new BusinessValidationError("Date Due is required.", "DUE_DATE_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            if (!string.IsNullOrWhiteSpace(dto.InvoiceRef))
            {
                bool duplicate = await _repository.ExistsAsync(dto.Project, dto.Contract, dto.InvoiceRef);
                if (duplicate)
                {
                    errors.Add(new BusinessValidationError(
                        "An invoice with this reference already exists for the selected project and contract.",
                        "INVOICE_REF_DUPLICATE"));
                    throw new BusinessValidationErrorException(errors);
                }
            }

            RadTrackInvoice newEntity = _mapper.Map<RadTrackInvoice>(dto);
            RadTrackInvoice created = await _repository.CreateAsync(newEntity);
            return _mapper.Map<RadTrackInvoiceDto>(created);
        }

       
        public async Task<RadTrackInvoiceDto> UpdateAsync(RadTrackInvoiceDto dto)
        {
            if (dto is null)
                throw new ArgumentException("Invoice DTO must not be null.", nameof(dto));

            var errors = new List<BusinessValidationError>();

           
            if (dto.InvoiceCounter <= 0)
                errors.Add(new BusinessValidationError("Invoice counter is required for update.", "INVOICE_COUNTER_REQUIRED"));

          
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));          
          
            
            if (!dto.DueDate.HasValue)
                errors.Add(new BusinessValidationError("Date Due is required.", "DUE_DATE_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            
            RadTrackInvoice existing = await _repository.GetByIdAsync(dto.InvoiceCounter)
                ?? throw new KeyNotFoundException($"Invoice with counter {dto.InvoiceCounter} was not found.");

           
            if (!string.IsNullOrWhiteSpace(dto.InvoiceRef))
            {
                bool duplicate = await _repository.ExistsAsync(
                    dto.Project,
                    dto.Contract,
                    dto.InvoiceRef,
                    excludeInvoiceCounter: dto.InvoiceCounter);

                if (duplicate)
                {
                    errors.Add(new BusinessValidationError(
                        "An invoice with this reference already exists for the selected project and contract.",
                        "INVOICE_REF_DUPLICATE"));
                    throw new BusinessValidationErrorException(errors);
                }
            }
            
            _mapper.Map(dto, existing);
            RadTrackInvoice updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<RadTrackInvoiceDto>(updated);
        }

       
        public async Task<bool> DeleteAsync(int invoiceCounter)
            => await _repository.DeleteAsync(invoiceCounter);

        
        public async Task<RadTrackInvoiceTotalsDto> GetTotalsAsync(RadTrackInvoiceFilter? filter)
        {
            RadTrackInvoiceTotals totals = await _repository.GetTotalsAsync(filter);
            return _mapper.Map<RadTrackInvoiceTotalsDto>(totals);
        }

        public async Task<bool> ExistsAsync(string? project, string? contract, string? invoiceRef, int? excludeInvoiceCounter = null)
            => await _repository.ExistsAsync(project, contract, invoiceRef, excludeInvoiceCounter);

        public Task<List<string>> GetProjectsAsync()  => _repository.GetProjectsAsync();
        public Task<List<int>>    GetYearsAsync()     => _repository.GetYearsAsync();
        public Task<List<string>> GetContractsAsync() => _repository.GetContractsAsync();
        public Task<List<string>> GetProgramsAsync()  => _repository.GetProgramsAsync();
    }
}
