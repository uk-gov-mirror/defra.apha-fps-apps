using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsApiClientTest
{
    public class FpsApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsApiClient _client;

        public FpsApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsApiClient(_http, _mapper);
        }

        [Fact]
        public void Constructor_Always_WiresFpsTestRCCostProperty()
        {
            Assert.NotNull(_client.FpsTestRCCost);
            Assert.IsType<FpsTestRCCostApiClient>(_client.FpsTestRCCost);
        }

        [Fact]
        public void Constructor_Always_WiresFpsTestRequirementRCCostProperty()
        {
            Assert.NotNull(_client.FpsTestRequirementRCCost);
            Assert.IsType<FpsTestRequirementRCCostApiClient>(_client.FpsTestRequirementRCCost);
        }

        [Fact]
        public void Constructor_Always_WiresFpsProjectAuditTrailProperty()
        {
            Assert.NotNull(_client.FpsProjectAuditTrail);
        }

        [Fact]
        public void Constructor_Always_WiresFpsTotalBusinessOverheadsProperty()
        {
            Assert.NotNull(_client.FpsTotalBusinessOverheads);
        }

        [Fact]
        public void FpsTestRCCost_ImplementsExpectedInterface()
        {
            Assert.IsAssignableFrom<IFpsTestRCCostApiClient>(_client.FpsTestRCCost);
        }

        [Fact]
        public void FpsTestRequirementRCCost_ImplementsExpectedInterface()
        {
            Assert.IsAssignableFrom<IFpsTestRequirementRCCostApiClient>(_client.FpsTestRequirementRCCost);
        }
    }
}
