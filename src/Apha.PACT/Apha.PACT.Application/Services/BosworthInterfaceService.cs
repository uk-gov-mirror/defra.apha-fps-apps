using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Interfaces;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class BosworthInterfaceService : IBosworthInterfaceService
    {
        private readonly IBosworthInterfaceRepository _repository;
        private readonly IMapper _mapper;

        public BosworthInterfaceService(IBosworthInterfaceRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TimePurchaseProjectDto>> GetTimePurchaseProjectAsync(string project)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(project))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var data = await _repository.GetTimePurchaseProjectAsync(project);
            return _mapper.Map<IEnumerable<TimePurchaseProjectDto>>(data);
        }

        public async Task<IEnumerable<TimeSaleProfitCentreDto>> GetTimeSaleProfitCentreAsync(string profitCentre)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(profitCentre))
                errors.Add(new BusinessValidationError("Profit Centre is required", "PROFIT_CENTRE_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var data = await _repository.GetTimeSaleProfitCentreAsync(profitCentre);
            return _mapper.Map<IEnumerable<TimeSaleProfitCentreDto>>(data);
        }

        public async Task<IEnumerable<TimeSaleWorkGroupDto>> GetTimeSaleWorkGroupAsync(string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("Work Group is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var data = await _repository.GetTimeSaleWorkGroupAsync(workGroup);
            return _mapper.Map<IEnumerable<TimeSaleWorkGroupDto>>(data);
        }

        public async Task<IEnumerable<TestSaleSellingWorkgroupDto>> GetTestSaleSellingWorkgroupAsync(string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("Work Group is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var data = await _repository.GetTestSaleSellingWorkgroupAsync(workGroup);
            return _mapper.Map<IEnumerable<TestSaleSellingWorkgroupDto>>(data);
        }

        public async Task<IEnumerable<TestSaleBuyingProjectDto>> GetTestSaleBuyingProjectAsync(string parentProject)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(parentProject))
                errors.Add(new BusinessValidationError("Parent Project is required", "PARENT_PROJECT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var data = await _repository.GetTestSaleBuyingProjectAsync(parentProject);
            return _mapper.Map<IEnumerable<TestSaleBuyingProjectDto>>(data);
        }
    }
}