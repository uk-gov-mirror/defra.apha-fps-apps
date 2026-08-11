using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.ReportGroupLinkServiceTest
{
    public class ReportGroupLinkServiceTests
    {
        private readonly IReportGroupLinkRepository _repository;
        private readonly IReportRepository _reportRepository;
        private readonly IReportGroupRepository _reportGroupRepository;
        private readonly IMapper _mapper;
        private readonly ReportGroupLinkService _service;

        public ReportGroupLinkServiceTests()
        {
            _repository = Substitute.For<IReportGroupLinkRepository>();
            _reportRepository = Substitute.For<IReportRepository>();
            _reportGroupRepository = Substitute.For<IReportGroupRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new ReportGroupLinkService(_repository, _reportRepository, _reportGroupRepository, _mapper);
        }

        [Fact]
        public async Task CreateAsync_DuplicateLink_ThrowsNamesBasedMessage()
        {
            // Arrange
            var dto = new ReportGroupLinkDto { ReportId = 10, GroupId = 20 };
            _repository.ReportGroupLinkExistsAsync(10, 20).Returns(true);
            _reportRepository.GetReportByIdAsync(10).Returns(new Report { Id = 10, ReportName = "Annual Report" });
            _reportGroupRepository.GetReportGroupByIdAsync(20).Returns(new ReportGroup { GroupId = 20, Description = "Finance" });

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateReportGroupLinkAsync(dto));

            // Assert
            Assert.Contains("Annual Report", ex.Message);
            Assert.Contains("Finance", ex.Message);
            await _repository.DidNotReceive().AddReportGroupLinkAsync(Arg.Any<ReportGroupLink>());
        }

        [Fact]
        public async Task DeleteAsync_MissingLink_ThrowsNamesBasedMessage()
        {
            // Arrange
            _repository.ReportGroupLinkExistsAsync(10, 20).Returns(false);
            _reportRepository.GetReportByIdAsync(10).Returns(new Report { Id = 10, ReportName = "Annual Report" });
            _reportGroupRepository.GetReportGroupByIdAsync(20).Returns(new ReportGroup { GroupId = 20, Description = "Finance" });

            // Act
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteReportGroupLinkAsync(10, 20));

            // Assert
            Assert.Contains("Annual Report", ex.Message);
            Assert.Contains("Finance", ex.Message);
        }
    }
}
