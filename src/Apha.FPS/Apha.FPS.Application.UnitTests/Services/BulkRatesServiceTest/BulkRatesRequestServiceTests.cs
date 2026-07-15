using Apha.FPS.Application.Services;
using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.BulkRatesServiceTest;

/// <summary>
/// Unit tests for <see cref="BulkRatesRequestService"/>.
/// Covers: status-guarded transitions, maker-checker enforcement, initiator-only guards,
/// and cancellation eligibility rules (US-XC-04).
/// </summary>
public class BulkRatesRequestServiceTests
{
    // ── Test fixtures ────────────────────────────────────────────────────────

    private const string JobName = "BulkTestRatesUpdate";
    private const int    FpsYear = 2027;

    private static readonly Guid     QueueId   = Guid.NewGuid();
    private static readonly Guid     ExecId    = Guid.NewGuid();
    private const           string   Initiator = "alice@test.com";
    private const           string   Approver  = "bob@test.com";

    // Entry helpers

    private static BulkRatesQueueEntry Entry(
        string status = "Initiated",
        string requestedBy = Initiator,
        string? approvedBy = null,
        string? rejectedBy = null,
        string? cancelledBy = null,
        string? configJson = null)
        => new()
        {
            JobQueueId        = QueueId,
            JobId             = 10,
            JobName           = JobName,
            StatusId          = 1,
            Status            = status,
            JobExecutionId    = ExecId,
            RequestedBy       = requestedBy,
            RequestedAtUtc    = DateTime.UtcNow.AddMinutes(-5),
            FpsYear           = FpsYear,
            ApprovedBy        = approvedBy,
            ApprovedAtUtc     = approvedBy != null ? DateTime.UtcNow : null,
            RejectedBy        = rejectedBy,
            ConfigurationJson = configJson
        };

    // SUT factory

    private static BulkRatesRequestService CreateService(
        IBulkRatesRepository? repo = null,
        IEventBridgePublisher? eb = null,
        IBulkRatesNotificationService? notif = null)
    {
        var r  = repo  ?? Substitute.For<IBulkRatesRepository>();
        var e  = eb    ?? Substitute.For<IEventBridgePublisher>();
        var n  = notif ?? Substitute.For<IBulkRatesNotificationService>();
        return new BulkRatesRequestService(
            r,
            new BulkRatesExcelParser(),
            new BulkRatesValidator(r),
            e, n,
            NullLogger<BulkRatesRequestService>.Instance);
    }

    // Repo stub that returns an entry and resolves status IDs

    private static IBulkRatesRepository RepoReturning(BulkRatesQueueEntry entry)
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRequestAsync(QueueId, Arg.Any<CancellationToken>()).Returns(entry);
        repo.GetJobIdByNameAsync(JobName, Arg.Any<CancellationToken>()).Returns((int?)10);
        repo.FpsYearExistsAsync(FpsYear, Arg.Any<CancellationToken>()).Returns(true);
        repo.GetStatusIdByNameAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((int?)42);
        repo.GetValidationErrorsAsync(QueueId, Arg.Any<CancellationToken>()).Returns(Array.Empty<StagingValidationError>() as IReadOnlyList<StagingValidationError>);
        repo.GetJobQueueLogsAsync(QueueId, Arg.Any<CancellationToken>()).Returns(Array.Empty<BulkRatesQueueLog>() as IReadOnlyList<BulkRatesQueueLog>);
        return repo;
    }

    // ── CreateRequestAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateRequest_WhenJobNameUnknown_ThrowsBusinessValidation()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetJobIdByNameAsync(JobName, Arg.Any<CancellationToken>()).Returns((int?)null);

        var svc = CreateService(repo);

        await svc.Invoking(s => s.CreateRequestAsync(JobName, FpsYear, Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*not a registered*");
    }

    [Fact]
    public async Task CreateRequest_WhenFpsYearDoesNotExist_ThrowsBusinessValidation()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetJobIdByNameAsync(JobName, Arg.Any<CancellationToken>()).Returns((int?)10);
        repo.FpsYearExistsAsync(FpsYear, Arg.Any<CancellationToken>()).Returns(false);

        var svc = CreateService(repo);

        await svc.Invoking(s => s.CreateRequestAsync(JobName, FpsYear, Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*does not exist*");
    }

    // ── ReleaseForApprovalAsync status guards ────────────────────────────────

    [Fact]
    public async Task Release_WhenStatusIsNotInitiated_ThrowsBusinessValidation()
    {
        var repo  = RepoReturning(Entry(status: "ReleasedForApproval"));
        var svc   = CreateService(repo);

        await svc.Invoking(s => s.ReleaseForApprovalAsync(QueueId, Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*Initiated*");
    }

    [Fact]
    public async Task Release_WhenCallerIsNotInitiator_ThrowsBusinessValidation()
    {
        var repo = RepoReturning(Entry(status: "Initiated", requestedBy: Initiator));
        var svc  = CreateService(repo);

        await svc.Invoking(s => s.ReleaseForApprovalAsync(QueueId, Approver))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*initiator*");
    }

    [Fact]
    public async Task Release_WhenNoFileUploaded_ThrowsBusinessValidation()
    {
        var repo = RepoReturning(Entry(status: "Initiated", configJson: null));
        var svc  = CreateService(repo);

        await svc.Invoking(s => s.ReleaseForApprovalAsync(QueueId, Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*No file*");
    }

    [Fact]
    public async Task Release_WhenBlockingValidationErrors_ThrowsBusinessValidation()
    {
        var configWithChecksum = """{"filename":"test.xlsx","checksum_sha256":"abc","upload_version":1}""";
        var repo  = RepoReturning(Entry(status: "Initiated", configJson: configWithChecksum));
        var errors = new[] { new StagingValidationError { Severity = "Error", ValidationMessage = "bad row" } };
        repo.GetValidationErrorsAsync(QueueId, Arg.Any<CancellationToken>()).Returns(errors as IReadOnlyList<StagingValidationError>);
        var svc = CreateService(repo);

        await svc.Invoking(s => s.ReleaseForApprovalAsync(QueueId, Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*blocking*");
    }

    // ── ApproveAsync maker-checker ───────────────────────────────────────────

    [Fact]
    public async Task Approve_WhenApproverIsInitiator_ThrowsMakerCheckerViolation()
    {
        var configWithChecksum = """{"filename":"test.xlsx","checksum_sha256":"abc","upload_version":1}""";
        var repo = RepoReturning(Entry(status: "ReleasedForApproval", configJson: configWithChecksum));
        var svc  = CreateService(repo);

        var ex = await svc.Invoking(s => s.ApproveAsync(QueueId, Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>();
        ex.Which.Errors.Should().Contain(e => e.Code == "MAKER_CHECKER_VIOLATION");
    }

    [Fact]
    public async Task Approve_WhenStatusIsNotReleasedForApproval_ThrowsBusinessValidation()
    {
        var repo = RepoReturning(Entry(status: "Initiated"));
        var svc  = CreateService(repo);

        await svc.Invoking(s => s.ApproveAsync(QueueId, Approver))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*ReleasedForApproval*");
    }

    [Fact]
    public async Task Approve_WhenChecksumMissing_ThrowsBusinessValidation()
    {
        var repo = RepoReturning(Entry(status: "ReleasedForApproval", configJson: null));
        var svc  = CreateService(repo);

        await svc.Invoking(s => s.ApproveAsync(QueueId, Approver))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*metadata*");
    }

    // ── RejectAsync maker-checker ────────────────────────────────────────────

    [Fact]
    public async Task Reject_WhenReasonIsEmpty_ThrowsBusinessValidation()
    {
        var repo = RepoReturning(Entry(status: "ReleasedForApproval"));
        var svc  = CreateService(repo);

        await svc.Invoking(s => s.RejectAsync(QueueId, Approver, ""))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*mandatory*");
    }

    [Fact]
    public async Task Reject_WhenRejectorIsInitiator_ThrowsMakerCheckerViolation()
    {
        var repo = RepoReturning(Entry(status: "ReleasedForApproval"));
        var svc  = CreateService(repo);

        var ex = await svc.Invoking(s => s.RejectAsync(QueueId, Initiator, "some reason"))
            .Should().ThrowAsync<BusinessValidationErrorException>();
        ex.Which.Errors.Should().Contain(e => e.Code == "MAKER_CHECKER_VIOLATION");
    }

    [Fact]
    public async Task Reject_WhenStatusIsNotReleasedForApproval_ThrowsBusinessValidation()
    {
        var repo = RepoReturning(Entry(status: "Approved"));
        var svc  = CreateService(repo);

        await svc.Invoking(s => s.RejectAsync(QueueId, Approver, "a reason"))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*ReleasedForApproval*");
    }

    // ── CancelAsync eligibility ──────────────────────────────────────────────

    [Fact]
    public async Task Cancel_WhenCallerIsNotInitiator_ThrowsBusinessValidation()
    {
        var repo = RepoReturning(Entry(status: "Initiated"));
        var svc  = CreateService(repo);

        await svc.Invoking(s => s.CancelAsync(QueueId, Approver, null))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*initiator*");
    }

    [Fact]
    public async Task Cancel_WhenStatusIsApproved_ThrowsBusinessValidation()
    {
        var repo = RepoReturning(Entry(status: "Approved"));
        var svc  = CreateService(repo);

        await svc.Invoking(s => s.CancelAsync(QueueId, Initiator, null))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*Initiated*");
    }

    [Theory]
    [InlineData("Initiated")]
    [InlineData("Rejected")]
    public async Task Cancel_WhenStatusIsInitiatedOrRejected_CallsCancelAndClearStaging(string status)
    {
        var repo = RepoReturning(Entry(status: status));
        var svc  = CreateService(repo);

        await svc.CancelAsync(QueueId, Initiator, null);

        await repo.Received(1).CancelAndClearStagingAsync(
            QueueId, JobName,
            Initiator, Arg.Any<DateTime>(), null, 42,
            Arg.Any<CancellationToken>());
    }

    // ── GetRequestAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetRequest_WhenNotFound_ReturnsNull()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRequestAsync(QueueId, Arg.Any<CancellationToken>()).Returns((BulkRatesQueueEntry?)null);
        var svc = CreateService(repo);

        var result = await svc.GetRequestAsync(QueueId);

        result.Should().BeNull();
    }

    // ── UploadFileAsync re-open semantics ────────────────────────────────────

    [Fact]
    public async Task Upload_WhenStatusIsRejected_AutoTransitionsToInitiated()
    {
        var repo = RepoReturning(Entry(status: "Rejected"));
        repo.GetStatusIdByNameAsync(Arg.Any<int>(), "Initiated", Arg.Any<CancellationToken>()).Returns((int?)5);
        repo.GetStatusIdByNameAsync(Arg.Any<int>(), "Rejected",  Arg.Any<CancellationToken>()).Returns((int?)7);
        var svc = CreateService(repo);

        // Provide a minimal valid-looking xlsx
        var xlsxBytes = BuildMinimalXlsx();
        await svc.UploadFileAsync(QueueId, xlsxBytes, "rates.xlsx", Initiator);

        await repo.Received(1).TransitionStatusAsync(QueueId, 1, 5, Arg.Any<CancellationToken>());
    }

    // ── Upload by non-initiator ──────────────────────────────────────────────

    [Fact]
    public async Task Upload_WhenCallerIsNotInitiator_ThrowsBusinessValidation()
    {
        var repo = RepoReturning(Entry(status: "Initiated"));
        var svc  = CreateService(repo);

        await svc.Invoking(s => s.UploadFileAsync(QueueId, [1, 2], "rates.xlsx", Approver))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*initiator*");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Creates a minimal BulkTestRatesUpdate xlsx in memory.</summary>
    private static byte[] BuildMinimalXlsx()
    {
        using var wb = new ClosedXML.Excel.XLWorkbook();
        var fec = wb.Worksheets.Add("FEC");
        fec.Cell(1, 1).Value = "TestCode";
        fec.Cell(1, 2).Value = "Unit Price VLA";
        fec.Cell(1, 3).Value = "Defra Unit Price";
        fec.Cell(1, 4).Value = "FEC New";
        fec.Cell(1, 5).Value = "Change";
        fec.Cell(1, 6).Value = "Item Description";
        fec.Cell(1, 7).Value = "Short Description";
        fec.Cell(1, 8).Value = "Owner";
        fec.Cell(1, 9).Value = "Comments";
        fec.Cell(2, 1).Value = "TC001";
        fec.Cell(2, 4).Value = 12.50;

        var agrup = wb.Worksheets.Add("AGRUP");
        agrup.Cell(1, 1).Value = "Test Code";
        agrup.Cell(1, 2).Value = "Buyer";
        agrup.Cell(1, 3).Value = "Agrup";
        agrup.Cell(1, 4).Value = "Agrup New";
        agrup.Cell(1, 5).Value = "Change";
        agrup.Cell(1, 6).Value = "No Required";
        agrup.Cell(1, 7).Value = "Date Created";
        agrup.Cell(1, 8).Value = "Active";
        agrup.Cell(1, 9).Value = "Comments";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
