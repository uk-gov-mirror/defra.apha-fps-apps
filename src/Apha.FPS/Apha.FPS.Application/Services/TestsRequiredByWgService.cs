using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class TestsRequiredByWgService : ITestsRequiredByWgService
    {
        private readonly ITestsRequiredByWgRepository _repository;
        private readonly IMapper _mapper;

        public TestsRequiredByWgService(ITestsRequiredByWgRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<TestsRequiredByWgDto>> GetTestsRequiredByWgAsync(string? profitCentre)
        {
            var entities = await _repository.GetTestsRequiredByWgAsync(profitCentre);
            return _mapper.Map<List<TestsRequiredByWgDto>>(entities);
        }
    }
}
