using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Handler;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.BulkRatesControllerTest
{
    /// <summary>
    /// First test class for <see cref="BulkRatesController"/> (this Web
    /// controller previously had zero coverage). Scoped to the request-scoped download
    /// actions for Staff/Animal — the controller's much larger pre-existing action surface
    /// (queue/create/upload/release/approve/etc.) predates this plan and is a separate concern.
    /// </summary>
    public class BulkRatesControllerTests
    {
        private readonly IBulkRatesService _bulkRatesService;
        private readonly ILogger<BulkRatesController> _logger;
        private readonly IFpsYearContext _fpsYearContext;
        private readonly IMapper _mapper;
        private readonly ITempDataDictionary _tempData;
        private readonly BulkRatesController _sut;

        public BulkRatesControllerTests()
        {
            _bulkRatesService = Substitute.For<IBulkRatesService>();
            _logger = Substitute.For<ILogger<BulkRatesController>>();
            _fpsYearContext = Substitute.For<IFpsYearContext>();
            _mapper = Substitute.For<IMapper>();

            _sut = new BulkRatesController(_bulkRatesService, _logger, _fpsYearContext, _mapper);
            _tempData = Substitute.For<ITempDataDictionary>();
            _sut.TempData = _tempData;
        }

        [Fact]
        public async Task DownloadStaffTestDataForRequest_WhenServiceSucceeds_ReturnsFileWithRequestScopedName()
        {
            var id = Guid.NewGuid();
            var bytes = new byte[] { 1, 2, 3 };
            _bulkRatesService.DownloadStaffTestDataForRequestAsync(id).Returns(bytes);

            var result = await _sut.DownloadStaffTestDataForRequest(id);

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Same(bytes, file.FileContents);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
            Assert.Equal($"Staff_TestRates_{id}.xlsx", file.FileDownloadName);
        }

        [Fact]
        public async Task DownloadStaffTestDataForRequest_WhenServiceThrows_RedirectsToDetailWithErrorMessage()
        {
            var id = Guid.NewGuid();
            _bulkRatesService.DownloadStaffTestDataForRequestAsync(id)
                .Returns(Task.FromException<byte[]>(new InvalidOperationException("boom")));

            var result = await _sut.DownloadStaffTestDataForRequest(id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(BulkRatesController.Detail), redirect.ActionName);
            Assert.Equal(id, redirect.RouteValues!["id"]);
            _tempData.Received()["ErrorMessage"] = "The Staff test data could not be downloaded. Please try again.";
        }

        [Fact]
        public async Task DownloadAnimalTestDataForRequest_WhenServiceSucceeds_ReturnsFileWithRequestScopedName()
        {
            var id = Guid.NewGuid();
            var bytes = new byte[] { 4, 5, 6 };
            _bulkRatesService.DownloadAnimalTestDataForRequestAsync(id).Returns(bytes);

            var result = await _sut.DownloadAnimalTestDataForRequest(id);

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Same(bytes, file.FileContents);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
            Assert.Equal($"Animal_TestRates_{id}.xlsx", file.FileDownloadName);
        }

        [Fact]
        public async Task DownloadAnimalTestDataForRequest_WhenServiceThrows_RedirectsToDetailWithErrorMessage()
        {
            var id = Guid.NewGuid();
            _bulkRatesService.DownloadAnimalTestDataForRequestAsync(id)
                .Returns(Task.FromException<byte[]>(new InvalidOperationException("boom")));

            var result = await _sut.DownloadAnimalTestDataForRequest(id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(BulkRatesController.Detail), redirect.ActionName);
            Assert.Equal(id, redirect.RouteValues!["id"]);
            _tempData.Received()["ErrorMessage"] = "The Animal test data could not be downloaded. Please try again.";
        }
    }
}
