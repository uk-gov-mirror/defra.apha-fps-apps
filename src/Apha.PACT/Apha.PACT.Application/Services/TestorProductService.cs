using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class TestorProductService : ITestorProductService
    {
        private readonly ITestorProductRepository _repository;
        private readonly ITestCapabilityRepository _testCapabilityRepository;
        private readonly IMapper _mapper;

        public TestorProductService(ITestorProductRepository repository, ITestCapabilityRepository testCapabilityRepository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _testCapabilityRepository = testCapabilityRepository ?? throw new ArgumentNullException(nameof(testCapabilityRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<TestorProductDto>> GetAllTestorProductsAsync()
        {
            var items = await _repository.GetAllTestorProductsAsync();
            return _mapper.Map<IEnumerable<TestorProductDto>>(items);


        }

        public async Task<PaginatedResult<TestorProductDto>> GetPagedTestOrProductsAsync(QueryParameters<string> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query parameters cannot be null.");
            }

            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedTestOrProductsAsync(parameters);

            if (pagedData == null)
            {
                throw new InvalidOperationException("Failed to retrieve paged test/product data from repository.");
            }

            return _mapper.Map<PaginatedResult<TestorProductDto>>(pagedData);
        }

        public async Task<TestorProductDto?> GetTestorProductByIdAsync(string itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                throw new ArgumentException("Item Code cannot be null or empty.", nameof(itemCode));
            }

            var entity = await _repository.GetTestOrProductByIdAsync(itemCode);
            return entity == null ? null : _mapper.Map<TestorProductDto>(entity);
        }

        public async Task<TestorProductDto> CreateTestorProductAsync(TestorProductDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Test/Product DTO cannot be null.");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.ItemCode))
            {
                throw new ArgumentException("Item Code is required.");
            }

            // Check for duplicate primary key
            var existing = await _repository.GetTestOrProductByIdAsync(dto.ItemCode);
            if (existing != null)
            {
                throw new InvalidOperationException($"A Test/Product with Item Code '{dto.ItemCode}' already exists.");
            }

            // Validate business rules
            ValidateTestOrProductDto(dto);

            var entity = _mapper.Map<TestorProduct>(dto);
            var created = await _repository.CreateTestOrProductAsync(entity);

            if (created == null)
            {
                throw new InvalidOperationException("Failed to create test/product.");
            }

            return _mapper.Map<TestorProductDto>(created);
        }

        public async Task<TestorProductDto> UpdateTestorProductAsync(TestorProductDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Test/Product DTO cannot be null.");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.ItemCode))
            {
                throw new ArgumentException("Item Code is required for update.");
            }

            // Verify entity exists before update
            var existingEntity = await _repository.GetTestOrProductByIdAsync(dto.ItemCode);
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"Test/Product with Item Code '{dto.ItemCode}' not found.");
            }

            // Validate business rules
            ValidateTestOrProductDto(dto);

            var entity = _mapper.Map<TestorProduct>(dto);
            var updated = await _repository.UpdateTestOrProductAsync(entity);

            if (updated == null)
            {
                throw new InvalidOperationException($"Failed to update test/product with Item Code '{dto.ItemCode}'.");
            }

            return _mapper.Map<TestorProductDto>(updated);
        }

        public async Task<bool> DeleteTestorProductAsync(string itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                throw new ArgumentException("Item Code cannot be null or empty.", nameof(itemCode));
            }
            var existingTestCapEntity = await _testCapabilityRepository.HasRelatedTestCapabilitiesValidRecordsAsync(itemCode);
            if(existingTestCapEntity != null)
            {
                throw new InvalidOperationException($"Cannot delete Test/Product with Item Code '{itemCode}' because it is referenced by a Test Capability.");
            }
            // Verify entity exists before deletion
            var existingEntity = await _repository.GetTestOrProductByIdAsync(itemCode);
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"Test/Product with Item Code '{itemCode}' not found for deletion.");
            }

            return await _repository.DeleteTestOrProductAsync(itemCode);
        }

        public async Task<IEnumerable<string>> GetOwnersAsync()
        {
            var owners = await _repository.GetOwnersAsync();

            if (owners == null)
            {
                throw new InvalidOperationException("Failed to retrieve owners from repository.");
            }

            return owners;
        }

        /// <summary>
        /// Validates TestOrProductDto business rules and data constraints.
        /// </summary>
        /// <param name="dto">The DTO to validate</param>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
        private void ValidateTestOrProductDto(TestorProductDto dto)
        {
            var validationErrors = new List<string>();

            // Validate ItemCode length
            if (!string.IsNullOrWhiteSpace(dto.ItemCode) && dto.ItemCode.Length > 20)
            {
                validationErrors.Add("Item Code cannot exceed 20 characters.");
            }

            // Validate ShortDescription length
            if (!string.IsNullOrWhiteSpace(dto.ShortDescription) && dto.ShortDescription.Length > 18)
            {
                validationErrors.Add("Short Description cannot exceed 18 characters.");
            }

            // Validate ItemDescription length
            if (!string.IsNullOrWhiteSpace(dto.ItemDescription) && dto.ItemDescription.Length > 200)
            {
                validationErrors.Add("Item Description cannot exceed 200 characters.");
            }

            // Validate TestManager length
            if (!string.IsNullOrWhiteSpace(dto.TestManager) && dto.TestManager.Length > 50)
            {
                validationErrors.Add("Test Manager cannot exceed 50 characters.");
            }

            // Validate Owner length
            if (!string.IsNullOrWhiteSpace(dto.Owner) && dto.Owner.Length > 2)
            {
                validationErrors.Add("Owner cannot exceed 2 characters.");
            }

            // Validate JobStatus length
            if (!string.IsNullOrWhiteSpace(dto.JobStatus) && dto.JobStatus.Length > 2)
            {
                validationErrors.Add("Job Status cannot exceed 2 characters.");
            }

            // Validate ChargeMethod length
            if (!string.IsNullOrWhiteSpace(dto.ChargeMethod) && dto.ChargeMethod.Length > 2)
            {
                validationErrors.Add("Charge Method cannot exceed 2 characters.");
            }

            // Validate DefraUnitPrice is non-negative
            if (dto.DefraUnitPrice < 0)
            {
                validationErrors.Add("DEFRA Unit Price cannot be negative.");
            }

            // Validate UnitPriceVla is non-negative if provided
            if (dto.UnitPriceVla.HasValue && dto.UnitPriceVla.Value < 0)
            {
                validationErrors.Add("Unit Price VLA cannot be negative.");
            }

            // Validate PriceAhvg is non-negative if provided
            if (dto.PriceAhvg.HasValue && dto.PriceAhvg.Value < 0)
            {
                validationErrors.Add("Price AHVG cannot be negative.");
            }

            // Validate FpsYear is within reasonable range
            if (dto.FpsYear < 2000 || dto.FpsYear > 2100)
            {
                validationErrors.Add("FPS Year must be between 2000 and 2100.");
            }

            // Throw exception if any validation errors found
            if (validationErrors.Count != 0)
            {
                throw new ArgumentException($"Validation failed: {string.Join(" ", validationErrors)}");
            }
        }

        // ── TestPriceCheck (frmTestPriceCheck — qryTestPriceZero) ──────────────────────────────

        public async Task<PaginatedResult<TestPriceCheckDto>> GetTestPriceCheckPagedAsync(
            QueryParameters<string> query,
            string priceFilter,
            string? owner)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetTestPriceCheckPagedAsync(parameters, priceFilter, owner);
            return _mapper.Map<PaginatedResult<TestPriceCheckDto>>(pagedData);
        }

        public async Task<TestPriceCheckDto?> GetTestPriceCheckByKeyAsync(string testCode, string jobCode)
        {
            var entity = await _repository.GetTestPriceCheckByKeyAsync(testCode, jobCode);
            return entity == null ? null : _mapper.Map<TestPriceCheckDto>(entity);
        }

        public async Task<bool> UpdateTestPriceCheckAsync(string testCode, string jobCode, TestPriceCheckDto dto)
            => await _repository.UpdateTestPriceCheckAsync(testCode, jobCode, dto.IsDefraProject, dto.TestPrice, dto.DefraUnitPrice);

        }
}
