using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPS.Api.UnitTests.Controller.BulkRatesControllerTest
{
    /// <summary>
    /// First test class for <see cref="BulkRatesController"/> — the
    /// controller previously had zero coverage. Scoped to the request-scoped download family
    /// (new Staff/Animal routes, plus the pre-existing FEC route they mirror), not the
    /// controller's full 20-action surface, which predates this plan and is a separate concern.
    /// </summary>
    public class BulkRatesControllerTests
    {
        private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IBulkRatesRequestService _service;
        private readonly IFpsRequestContext _requestContext;
        private readonly BulkRatesController _sut;

        public BulkRatesControllerTests()
        {
            _service = Substitute.For<IBulkRatesRequestService>();
            _requestContext = Substitute.For<IFpsRequestContext>();
            _sut = new BulkRatesController(_service, _requestContext);
        }

        [Fact]
        public async Task DownloadFecTestDataAsync_ReturnsFileWithRequestScopedName()
        {
            var jobExecutionId = Guid.NewGuid();
            var bytes = new byte[] { 1, 2, 3 };
            _service.DownloadFecTestDataAsync(jobExecutionId, Arg.Any<CancellationToken>()).Returns(bytes);

            var result = await _sut.DownloadFecTestDataAsync(jobExecutionId, CancellationToken.None);

            var file = result.Should().BeOfType<FileContentResult>().Subject;
            file.FileContents.Should().BeSameAs(bytes);
            file.ContentType.Should().Be(ContentType);
            file.FileDownloadName.Should().Be($"FEC_TestRates_{jobExecutionId}.xlsx");
            await _service.Received(1).DownloadFecTestDataAsync(jobExecutionId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DownloadStaffTestDataForRequestAsync_ReturnsFileWithRequestScopedName()
        {
            var jobExecutionId = Guid.NewGuid();
            var bytes = new byte[] { 4, 5, 6 };
            _service.DownloadStaffTestDataAsync(jobExecutionId, Arg.Any<CancellationToken>()).Returns(bytes);

            var result = await _sut.DownloadStaffTestDataForRequestAsync(jobExecutionId, CancellationToken.None);

            var file = result.Should().BeOfType<FileContentResult>().Subject;
            file.FileContents.Should().BeSameAs(bytes);
            file.ContentType.Should().Be(ContentType);
            file.FileDownloadName.Should().Be($"Staff_TestRates_{jobExecutionId}.xlsx");
            await _service.Received(1).DownloadStaffTestDataAsync(jobExecutionId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DownloadAnimalTestDataForRequestAsync_ReturnsFileWithRequestScopedName()
        {
            var jobExecutionId = Guid.NewGuid();
            var bytes = new byte[] { 7, 8, 9 };
            _service.DownloadAnimalTestDataAsync(jobExecutionId, Arg.Any<CancellationToken>()).Returns(bytes);

            var result = await _sut.DownloadAnimalTestDataForRequestAsync(jobExecutionId, CancellationToken.None);

            var file = result.Should().BeOfType<FileContentResult>().Subject;
            file.FileContents.Should().BeSameAs(bytes);
            file.ContentType.Should().Be(ContentType);
            file.FileDownloadName.Should().Be($"Animal_TestRates_{jobExecutionId}.xlsx");
            await _service.Received(1).DownloadAnimalTestDataAsync(jobExecutionId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DownloadStaffTestDataForRequestAsync_DoesNotCallAnimalOrFecService()
        {
            // Route-level safety: a Staff download route must never resolve to a
            // different job type's service method — RequireJobName is the actual runtime
            // guard inside DownloadStaffTestDataAsync itself; this just proves the controller
            // wires the Staff route to the Staff service method, not a shared/ambiguous one.
            var jobExecutionId = Guid.NewGuid();
            _service.DownloadStaffTestDataAsync(jobExecutionId, Arg.Any<CancellationToken>()).Returns([]);

            await _sut.DownloadStaffTestDataForRequestAsync(jobExecutionId, CancellationToken.None);

            await _service.DidNotReceive().DownloadAnimalTestDataAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            await _service.DidNotReceive().DownloadFecTestDataAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        }
    }
}
