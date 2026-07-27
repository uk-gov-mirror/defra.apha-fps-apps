using Apha.Common.BulkRates.Validation;
using Apha.Common.Utilities.ExcelExport;
using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Application.Services;
using NSubstitute.ExceptionExtensions;
using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

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
        string? uploadChecksum = null,
        int? activeDownloadVersion = null)
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
            UploadFilename       = uploadChecksum != null ? "test.xlsx" : null,
            UploadChecksumSha256 = uploadChecksum,
            UploadVersion        = uploadChecksum != null ? 1 : null,
            // A real upload always has row counts recorded alongside the checksum
            // (see UploadFileAsync's ReplaceStaging + UpdateUploadMetadata pairing) —
            // default to a non-zero total so callers testing post-upload behaviour
            // don't also need to fake this separately.
            UploadRowCountsJson  = uploadChecksum != null ? """{"total":1}""" : null,
            ActiveDownloadVersion = activeDownloadVersion
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
            new BulkRatesValidator(r, new BulkRatesValidationService()),
            e, n,
            Substitute.For<IExcelExportService>(),
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

        // DR-VAL-01 wiring (BulkRatesValidator.BuildContextAsync / BuildFreezeAsync) — default
        // to empty so Upload/Release exercise the real BulkRatesValidationService without any
        // live/staged data unless a test overrides one of these.
        repo.GetFecRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>()).Returns(Array.Empty<FecStagingRow>() as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>()).Returns(Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
        repo.GetExistingProjectCodesAsync(Arg.Any<IEnumerable<string>>(), FpsYear, Arg.Any<CancellationToken>()).Returns(new HashSet<string>(StringComparer.OrdinalIgnoreCase) as IReadOnlySet<string>);
        repo.GetExistingCapabilityPairsAsync(Arg.Any<IEnumerable<(string, string)>>(), FpsYear, Arg.Any<CancellationToken>()).Returns(new HashSet<(string, string)>() as IReadOnlySet<(string, string)>);
        repo.GetFecSnapshotRowsAsync(QueueId, Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Array.Empty<FecStagingRow>() as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupSnapshotRowsAsync(QueueId, Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
        repo.GetFecStagingRowsAsync(QueueId, Arg.Any<CancellationToken>()).Returns(Array.Empty<FecStagingRow>() as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupStagingRowsAsync(QueueId, Arg.Any<CancellationToken>()).Returns(Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
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
        var repo = RepoReturning(Entry(status: "Initiated", uploadChecksum: null));
        var svc  = CreateService(repo);

        await svc.Invoking(s => s.ReleaseForApprovalAsync(QueueId, Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*No file*");
    }

    [Fact]
    public async Task Release_WhenBlockingValidationErrors_ThrowsBusinessValidation()
    {
        var repo  = RepoReturning(Entry(status: "Initiated", uploadChecksum: "abc"));
        var errors = new[] { new StagingValidationError { Severity = "Error", ValidationMessage = "bad row" } };
        repo.GetValidationErrorsAsync(QueueId, Arg.Any<CancellationToken>()).Returns(errors as IReadOnlyList<StagingValidationError>);
        var svc = CreateService(repo);

        await svc.Invoking(s => s.ReleaseForApprovalAsync(QueueId, Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*blocking*");
    }

    // ── Release: DR-API-07 re-validation + freeze ────────────────────────────

    [Fact]
    public async Task Release_WhenFecJobValidationClean_FreezesCalculatedActionsAndTransitions()
    {
        var repo = RepoReturning(Entry(status: "Initiated", uploadChecksum: "abc"));
        repo.GetFecStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(new[] { new FecStagingRow { TestCode = "TC001", FecNewRate = 15m } } as IReadOnlyList<FecStagingRow>);
        repo.GetFecRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 10m, DefraUnitPrice = 10m } } as IReadOnlyList<FecStagingRow>);
        var svc = CreateService(repo);

        await svc.ReleaseForApprovalAsync(QueueId, Initiator);

        await repo.Received(1).FreezeStagingCalculatedActionsAsync(
            QueueId, 1,
            Arg.Is<IReadOnlyList<BulkRatesFreezeEntry>>(list =>
                list.Count == 1 && list[0].TestCode == "TC001" && list[0].CalculatedAction == ValidationCalculatedAction.Update),
            Arg.Any<IReadOnlyList<BulkRatesFreezeEntry>>(),
            Arg.Any<CancellationToken>());
        await repo.Received(1).TransitionStatusAsync(QueueId, 1, 42, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Release_WhenReleaseTimeRevalidationFindsNewBlockingError_ThrowsAndDoesNotTransitionOrFreeze()
    {
        // Staged AGRUP row references a TestCode absent from both live FEC and staged FEC —
        // a fresh re-validation against current data catches this even though the errors
        // recorded at upload time (GetValidationErrorsAsync, stubbed empty) were clean.
        var repo = RepoReturning(Entry(status: "Initiated", uploadChecksum: "abc"));
        repo.GetAgrupStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(new[] { new AgrupStagingRow { TestCode = "GONE", Buyer = "B1", AgrupNew = 10m } } as IReadOnlyList<AgrupStagingRow>);
        var svc = CreateService(repo);

        await svc.Invoking(s => s.ReleaseForApprovalAsync(QueueId, Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>();

        await repo.DidNotReceive().TransitionStatusAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await repo.DidNotReceive().FreezeStagingCalculatedActionsAsync(
            Arg.Any<Guid>(), Arg.Any<int>(),
            Arg.Any<IReadOnlyList<BulkRatesFreezeEntry>>(), Arg.Any<IReadOnlyList<BulkRatesFreezeEntry>>(),
            Arg.Any<CancellationToken>());
    }

    // §7.4: release must be blocked by a request-level missing-downloaded-key error, distinct
    // from an ordinary per-row error — proves DR-API-06/DR-VAL-01's MISSING_DOWNLOADED_KEY finding
    // (already asserted IsRequestLevel==true at the shared-service unit level) also gates release,
    // not just display.
    [Fact]
    public async Task Release_WhenDownloadedFecKeyMissingFromStaging_ThrowsWithRequestLevelMissingKeyError()
    {
        var repo = RepoReturning(Entry(status: "Initiated", uploadChecksum: "abc", activeDownloadVersion: 1));
        // Downloaded snapshot recorded "TC-MISSING" at download time; staged FEC rows default
        // to empty (RepoReturning) — the re-upload silently dropped it.
        repo.GetFecSnapshotRowsAsync(QueueId, 1, Arg.Any<CancellationToken>())
            .Returns(new[] { new FecStagingRow { TestCode = "TC-MISSING", DefraUnitPrice = 12m } } as IReadOnlyList<FecStagingRow>);
        var svc = CreateService(repo);

        var ex = await svc.Invoking(s => s.ReleaseForApprovalAsync(QueueId, Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>();

        ex.Which.Errors.Should().ContainSingle(e => e.Code == "MISSING_DOWNLOADED_KEY");
        await repo.DidNotReceive().TransitionStatusAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await repo.DidNotReceive().FreezeStagingCalculatedActionsAsync(
            Arg.Any<Guid>(), Arg.Any<int>(),
            Arg.Any<IReadOnlyList<BulkRatesFreezeEntry>>(), Arg.Any<IReadOnlyList<BulkRatesFreezeEntry>>(),
            Arg.Any<CancellationToken>());
    }

    // ── ApproveAsync maker-checker ───────────────────────────────────────────

    [Fact]
    public async Task Approve_WhenApproverIsInitiator_Succeeds()
    {
        // Maker-checker enforcement is temporarily disabled — see BulkRatesRequestService.ApproveAsync.
        var repo = RepoReturning(Entry(status: "ReleasedForApproval", uploadChecksum: "abc"));
        var svc  = CreateService(repo);

        await svc.ApproveAsync(QueueId, Initiator);

        await repo.Received(1).SetApprovalAsync(
            QueueId, ExecId,
            Initiator, Arg.Any<DateTime>(),
            Initiator, Arg.Any<DateTime>(),
            42, Arg.Any<CancellationToken>());
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
        var repo = RepoReturning(Entry(status: "ReleasedForApproval", uploadChecksum: null));
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
    public async Task Reject_WhenRejectorIsInitiator_Succeeds()
    {
        // Maker-checker enforcement is temporarily disabled — see BulkRatesRequestService.RejectAsync.
        var repo = RepoReturning(Entry(status: "ReleasedForApproval"));
        var svc  = CreateService(repo);

        await svc.RejectAsync(QueueId, Initiator, "some reason");

        await repo.Received(1).SetRejectionAsync(
            QueueId, Initiator, Arg.Any<DateTime>(),
            "some reason", 42, Arg.Any<CancellationToken>());
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
    [InlineData("Failed")]
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
        // Fec upload must carry a download-version that matches the request's active
        // download (DR-VAL-03) — give the entry an active version and embed the same
        // version in the workbook's protected metadata sheet.
        var repo = RepoReturning(Entry(status: "Rejected", activeDownloadVersion: 1));
        repo.GetStatusIdByNameAsync(Arg.Any<int>(), "Initiated", Arg.Any<CancellationToken>()).Returns((int?)5);
        repo.GetStatusIdByNameAsync(Arg.Any<int>(), "Rejected",  Arg.Any<CancellationToken>()).Returns((int?)7);
        var svc = CreateService(repo);

        // Provide a minimal valid-looking xlsx
        var xlsxBytes = BuildMinimalXlsx(downloadVersion: 1);
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

    // ── Upload active-download-version contract (DR-VAL-03) ─────────────────

    [Fact]
    public async Task Upload_WhenWorkbookVersionMatchesActive_Succeeds()
    {
        var repo = RepoReturning(Entry(status: "Initiated", activeDownloadVersion: 3));
        var svc  = CreateService(repo);

        var xlsxBytes = BuildMinimalXlsx(downloadVersion: 3);
        var result = await svc.UploadFileAsync(QueueId, xlsxBytes, "rates.xlsx", Initiator);

        result.JobQueueId.Should().Be(QueueId);
    }

    [Fact]
    public async Task Upload_WhenWorkbookVersionDoesNotMatchActive_ThrowsStaleDownloadVersion()
    {
        var repo = RepoReturning(Entry(status: "Initiated", activeDownloadVersion: 2));
        var svc  = CreateService(repo);

        // Workbook carries version 1, but the request's active download has moved on to 2
        // (e.g. a second download was generated after this one).
        var xlsxBytes = BuildMinimalXlsx(downloadVersion: 1);

        var ex = await svc.Invoking(s => s.UploadFileAsync(QueueId, xlsxBytes, "rates.xlsx", Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>();
        ex.Which.Errors.Should().Contain(e => e.Code == "STALE_DOWNLOAD_VERSION");
    }

    [Fact]
    public async Task Upload_WhenWorkbookHasNoDownloadVersionMetadata_ThrowsStaleDownloadVersion()
    {
        // A request that has an active download but the uploaded workbook was never
        // generated by DownloadFecTestDataAsync (no protected metadata sheet at all).
        var repo = RepoReturning(Entry(status: "Initiated", activeDownloadVersion: 1));
        var svc  = CreateService(repo);

        var xlsxBytes = BuildMinimalXlsx(); // no metadata sheet

        var ex = await svc.Invoking(s => s.UploadFileAsync(QueueId, xlsxBytes, "rates.xlsx", Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>();
        ex.Which.Errors.Should().Contain(e => e.Code == "STALE_DOWNLOAD_VERSION");
    }

    [Fact]
    public async Task Upload_WhenRequestHasNoActiveDownloadYet_ThrowsStaleDownloadVersion()
    {
        // No download has ever been generated for this request (ActiveDownloadVersion is
        // null) — even a workbook that happens to carry a version must be rejected, since
        // there is no active version for it to match.
        var repo = RepoReturning(Entry(status: "Initiated", activeDownloadVersion: null));
        var svc  = CreateService(repo);

        var xlsxBytes = BuildMinimalXlsx(downloadVersion: 1);

        var ex = await svc.Invoking(s => s.UploadFileAsync(QueueId, xlsxBytes, "rates.xlsx", Initiator))
            .Should().ThrowAsync<BusinessValidationErrorException>();
        ex.Which.Errors.Should().Contain(e => e.Code == "STALE_DOWNLOAD_VERSION");
    }

    [Fact]
    public async Task Upload_WhenJobIsNotFec_SkipsDownloadVersionCheck()
    {
        // Staff/Animal workbooks are never generated with the protected metadata sheet
        // (only DownloadFecTestDataAsync writes it), so the contract must not apply to them.
        var staffEntry = Entry(status: "Initiated", activeDownloadVersion: null);
        staffEntry.JobName = Apha.Common.Constants.BulkRatesJobNames.Staff;
        var repo = RepoReturning(staffEntry);
        var svc  = CreateService(repo);

        using var wb = new ClosedXML.Excel.XLWorkbook();
        var staff = wb.Worksheets.Add("Staff");
        staff.Cell(1, 1).Value = "PcGrade";
        staff.Cell(1, 2).Value = "Pay Rate";
        staff.Cell(1, 3).Value = "NPR";
        staff.Cell(1, 4).Value = "OHR";
        using var ms = new MemoryStream();
        wb.SaveAs(ms);

        var result = await svc.UploadFileAsync(QueueId, ms.ToArray(), "staff.xlsx", Initiator);

        result.JobQueueId.Should().Be(QueueId);
    }

    // ── GetStagingDataAsync classification (DR-VAL-01 calculated action vs "Deleted") ──

    [Fact]
    public async Task GetStagingData_WhenJobNameIsUnrecognised_ReturnsEmpty()
    {
        // Staff/Animal are "not Fec" too, but have their own real staging diff (see the
        // GetStaffStagingData/GetAnimalStagingData tests) — this covers the defensive
        // fallback for a job name that is none of the three known ones.
        var unknownJobEntry = Entry(status: "Initiated", uploadChecksum: "abc");
        unknownJobEntry.JobName = "SomeUnrecognisedJob";
        var repo = RepoReturning(unknownJobEntry);
        var svc = CreateService(repo);

        var result = await svc.GetStagingDataAsync(QueueId);

        result.FecRows.Should().BeEmpty();
        result.AgrupRows.Should().BeEmpty();
        await repo.DidNotReceive().GetFecStagingRowsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStagingData_ClassifiesNewTestCodeAsInserted()
    {
        var repo = RepoReturning(Entry(status: "Initiated", uploadChecksum: "abc"));
        repo.GetFecStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(new[] { new FecStagingRow { TestCode = "T001", FecNewRate = 10.1m } } as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
        repo.GetFecRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<FecStagingRow>() as IReadOnlyList<FecStagingRow>);
        var svc = CreateService(repo);

        var result = await svc.GetStagingDataAsync(QueueId);

        result.FecRows.Should().ContainSingle(r => r.TestCode == "T001" && r.Status == "Insert");
    }

    [Fact]
    public async Task GetStagingData_ClassifiesChangedRateAsUpdated()
    {
        var repo = RepoReturning(Entry(status: "Initiated", uploadChecksum: "abc"));
        repo.GetFecStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(new[] { new FecStagingRow { TestCode = "T002", FecNewRate = 20.2m } } as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
        repo.GetFecRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(new[] { new FecStagingRow { TestCode = "T002", DefraUnitPrice = 15.0m } } as IReadOnlyList<FecStagingRow>);
        var svc = CreateService(repo);

        var result = await svc.GetStagingDataAsync(QueueId);

        result.FecRows.Should().ContainSingle(r => r.TestCode == "T002" && r.Status == "Update");
    }

    [Fact]
    public async Task GetStagingData_ClassifiesUnchangedRateAsNoChange()
    {
        // DR-UI-03: No Change rows are shown too (not hidden), labelled from the same
        // DR-VAL-01 classification as Insert/Update/Zero-Rate Withdrawal.
        var repo = RepoReturning(Entry(status: "Initiated", uploadChecksum: "abc"));
        repo.GetFecStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(new[] { new FecStagingRow { TestCode = "T003", FecNewRate = 30.0m } } as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
        repo.GetFecRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(new[] { new FecStagingRow { TestCode = "T003", UnitPriceVla = 30.0m, DefraUnitPrice = 30.0m } } as IReadOnlyList<FecStagingRow>);
        var svc = CreateService(repo);

        var result = await svc.GetStagingDataAsync(QueueId);

        result.FecRows.Should().ContainSingle(r => r.TestCode == "T003" && r.Status == "No Change");
    }

    [Fact]
    public async Task GetStagingData_ClassifiesLiveTestCodeMissingFromUploadAsDeleted()
    {
        var repo = RepoReturning(Entry(status: "Initiated", uploadChecksum: "abc"));
        repo.GetFecStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<FecStagingRow>() as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
        repo.GetFecRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(new[] { new FecStagingRow { TestCode = "T004", DefraUnitPrice = 40.0m } } as IReadOnlyList<FecStagingRow>);
        var svc = CreateService(repo);

        var result = await svc.GetStagingDataAsync(QueueId);

        result.FecRows.Should().ContainSingle(r => r.TestCode == "T004" && r.Status == "Deleted");
    }

    [Fact]
    public async Task GetStagingData_WhenRequestIsCompleted_DoesNotFloodGridWithLiveRowsAsDeleted()
    {
        // Staging rows are purged after a successful commit (BulkTestRatesService step 5,
        // spec §10.6), so GetFecStagingRowsAsync/GetAgrupStagingRowsAsync legitimately come
        // back empty here — that must not be read as "every live row was deleted": nothing
        // was actually removed, the diff source data is just gone. The live catalog must not
        // even be queried, since the diff is skipped entirely once Completed.
        var repo = RepoReturning(Entry(status: "Completed", uploadChecksum: "abc"));
        repo.GetFecRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(new[] { new FecStagingRow { TestCode = "T005", DefraUnitPrice = 50.0m } } as IReadOnlyList<FecStagingRow>);
        var svc = CreateService(repo);

        var result = await svc.GetStagingDataAsync(QueueId);

        result.FecRows.Should().BeEmpty();
        result.AgrupRows.Should().BeEmpty();
        await repo.DidNotReceive().GetFecRowsForExportAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        await repo.DidNotReceive().GetAgrupRowsForExportAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    // ── DownloadFecTestDataAsync (DR-UI-01) ──────────────────────────────────
    //
    // Verification checklist per fec-bulk-rates-plan-05-differential-remediation.md §tracker:
    //  (1) snapshot header created with status Generating
    //  (2) snapshot rows persisted from authoritative live data
    //  (3) workbook generated from persisted snapshot, not a second live query
    //  (4) header moved to Ready only after successful workbook creation
    //  (5) active_download_version updated only on success (MarkDownloadReadyAsync called)
    //  (6) failure leaves the previous active version unchanged (MarkDownloadFailedAsync called, not Ready)
    //  (7) downloaded workbook contains matching protected download_version metadata
    //  (8) FEC and AGRUP workbook rows exactly match the persisted snapshot
    //  (9) repeat download creates a higher version
    // (10) single-active-request guard: non-Initiated/Rejected status throws

    private static IBulkRatesRepository RepoForDownload(
        BulkRatesQueueEntry entry,
        int nextVersion = 1,
        IReadOnlyList<FecStagingRow>? liveFec = null,
        IReadOnlyList<AgrupStagingRow>? liveAgrup = null,
        IReadOnlyList<FecStagingRow>? snapshotFec = null,
        IReadOnlyList<AgrupStagingRow>? snapshotAgrup = null)
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRequestAsync(entry.JobExecutionId, Arg.Any<CancellationToken>()).Returns(entry);
        repo.GetNextDownloadVersionAsync(entry.JobQueueId, Arg.Any<CancellationToken>()).Returns(nextVersion);
        repo.GetFecRowsForExportAsync(entry.FpsYear, Arg.Any<CancellationToken>())
            .Returns(liveFec ?? Array.Empty<FecStagingRow>() as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupRowsForExportAsync(entry.FpsYear, Arg.Any<CancellationToken>())
            .Returns(liveAgrup ?? Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
        repo.GetFecSnapshotRowsAsync(entry.JobQueueId, nextVersion, Arg.Any<CancellationToken>())
            .Returns(snapshotFec ?? liveFec ?? Array.Empty<FecStagingRow>() as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupSnapshotRowsAsync(entry.JobQueueId, nextVersion, Arg.Any<CancellationToken>())
            .Returns(snapshotAgrup ?? liveAgrup ?? Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
        return repo;
    }

    private static BulkRatesRequestService CreateServiceWithExcel(
        IBulkRatesRepository repo,
        IExcelExportService? excel = null)
    {
        var r  = repo;
        var e  = Substitute.For<IEventBridgePublisher>();
        var n  = Substitute.For<IBulkRatesNotificationService>();
        var ex = excel ?? DefaultExcel();
        return new BulkRatesRequestService(
            r,
            new BulkRatesExcelParser(),
            new BulkRatesValidator(r, new BulkRatesValidationService()),
            e, n, ex,
            NullLogger<BulkRatesRequestService>.Instance);
    }

    private static IExcelExportService DefaultExcel()
    {
        var ex = Substitute.For<IExcelExportService>();
        ex.ExportToExcelMultiSheet(Arg.Any<IEnumerable<ExcelSheetDefinition>>(), Arg.Any<IReadOnlyDictionary<string, string>>())
          .Returns([1, 2, 3]);
        return ex;
    }

    // (10) Single-active-request guard: disallowed statuses throw

    [Theory]
    [InlineData("ReleasedForApproval")]
    [InlineData("Approved")]
    [InlineData("Cancelled")]
    [InlineData("Failed")]
    public async Task Download_WhenStatusIsNotDownloadable_ThrowsBusinessValidation(string status)
    {
        var entry = Entry(status: status);
        var repo  = RepoForDownload(entry);
        var svc   = CreateServiceWithExcel(repo);

        await svc.Invoking(s => s.DownloadFecTestDataAsync(entry.JobExecutionId))
            .Should().ThrowAsync<BusinessValidationErrorException>()
            .WithMessage("*Initiated*");
    }

    // (1) Snapshot header created with status Generating (CreateDownloadSnapshotAsync called)

    [Fact]
    public async Task Download_CreatesSnapshotHeader_WithGeneratingStatus()
    {
        var entry = Entry(status: "Initiated");
        var repo  = RepoForDownload(entry, nextVersion: 1);
        var svc   = CreateServiceWithExcel(repo);

        await svc.DownloadFecTestDataAsync(entry.JobExecutionId);

        await repo.Received(1).CreateDownloadSnapshotAsync(
            entry.JobQueueId,
            1,
            Arg.Any<IReadOnlyList<FecStagingRow>>(),
            Arg.Any<IReadOnlyList<AgrupStagingRow>>(),
            Arg.Any<CancellationToken>());
    }

    // (2) Snapshot rows persisted from authoritative live data (live data passed to CreateDownloadSnapshotAsync)

    [Fact]
    public async Task Download_PersistsLiveDataToSnapshot()
    {
        var liveFec   = new[] { new FecStagingRow { TestCode = "T100", FecNewRate = 99.9m } } as IReadOnlyList<FecStagingRow>;
        var liveAgrup = new[] { new AgrupStagingRow { TestCode = "T100", Buyer = "B1", Agrup = 5.0m } } as IReadOnlyList<AgrupStagingRow>;
        var entry = Entry(status: "Initiated");
        var repo  = RepoForDownload(entry, liveFec: liveFec, liveAgrup: liveAgrup);
        var svc   = CreateServiceWithExcel(repo);

        await svc.DownloadFecTestDataAsync(entry.JobExecutionId);

        await repo.Received(1).CreateDownloadSnapshotAsync(
            entry.JobQueueId, 1,
            Arg.Is<IReadOnlyList<FecStagingRow>>(rows => rows.Count == 1 && rows[0].TestCode == "T100"),
            Arg.Is<IReadOnlyList<AgrupStagingRow>>(rows => rows.Count == 1 && rows[0].Buyer == "B1"),
            Arg.Any<CancellationToken>());
    }

    // (3) Workbook generated from snapshot rows, not a second live query
    //     (GetFecRowsForExportAsync and GetAgrupRowsForExportAsync each called exactly once,
    //      GetFecSnapshotRowsAsync/GetAgrupSnapshotRowsAsync called for workbook generation)

    [Fact]
    public async Task Download_GeneratesWorkbookFromSnapshot_NotSecondLiveQuery()
    {
        var entry = Entry(status: "Initiated");
        var repo  = RepoForDownload(entry, nextVersion: 1);
        var svc   = CreateServiceWithExcel(repo);

        await svc.DownloadFecTestDataAsync(entry.JobExecutionId);

        // Live data queried exactly once (to build the snapshot), not again for workbook generation
        await repo.Received(1).GetFecRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>());
        await repo.Received(1).GetAgrupRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>());
        // Snapshot rows read back for workbook
        await repo.Received(1).GetFecSnapshotRowsAsync(entry.JobQueueId, 1, Arg.Any<CancellationToken>());
        await repo.Received(1).GetAgrupSnapshotRowsAsync(entry.JobQueueId, 1, Arg.Any<CancellationToken>());
    }

    // (4) Header moved to Ready only after successful workbook creation

    [Fact]
    public async Task Download_OnSuccess_MarksHeaderReady()
    {
        var entry = Entry(status: "Initiated");
        var repo  = RepoForDownload(entry, nextVersion: 1);
        var svc   = CreateServiceWithExcel(repo);

        await svc.DownloadFecTestDataAsync(entry.JobExecutionId);

        await repo.Received(1).MarkDownloadReadyAsync(entry.JobQueueId, 1, Arg.Any<CancellationToken>());
        await repo.DidNotReceive().MarkDownloadFailedAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    // (5) active_download_version updated only on success (implicit: MarkDownloadReadyAsync is the one call
    //     that writes active_download_version — test (4) already covers this path; this test makes the
    //     contrast explicit by checking MarkDownloadReadyAsync is NOT called on failure)

    // (6) Failure leaves the previous active version unchanged

    [Fact]
    public async Task Download_WhenWorkbookGenerationFails_MarksHeaderFailed_NotReady()
    {
        var entry    = Entry(status: "Initiated");
        var repo     = RepoForDownload(entry, nextVersion: 2);
        var faultyEx = Substitute.For<IExcelExportService>();
        faultyEx.ExportToExcelMultiSheet(Arg.Any<IEnumerable<ExcelSheetDefinition>>(), Arg.Any<IReadOnlyDictionary<string, string>>())
                .Throws(new InvalidOperationException("workbook generation exploded"));
        var svc = CreateServiceWithExcel(repo, faultyEx);

        await svc.Invoking(s => s.DownloadFecTestDataAsync(entry.JobExecutionId))
            .Should().ThrowAsync<InvalidOperationException>();

        await repo.Received(1).MarkDownloadFailedAsync(entry.JobQueueId, 2, Arg.Any<CancellationToken>());
        await repo.DidNotReceive().MarkDownloadReadyAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    // (7) Protected download_version metadata embedded in workbook

    [Fact]
    public async Task Download_EmbedsDowloadVersionInWorkbookMetadata()
    {
        var entry    = Entry(status: "Initiated");
        var repo     = RepoForDownload(entry, nextVersion: 3);
        Dictionary<string, string>? capturedMetadata = null;
        var excel = Substitute.For<IExcelExportService>();
        excel.ExportToExcelMultiSheet(
                Arg.Any<IEnumerable<ExcelSheetDefinition>>(),
                Arg.Do<IReadOnlyDictionary<string, string>>(m => capturedMetadata = new Dictionary<string, string>(m)))
             .Returns([0xFF, 0xFE]);
        var svc = CreateServiceWithExcel(repo, excel);

        await svc.DownloadFecTestDataAsync(entry.JobExecutionId);

        capturedMetadata.Should().NotBeNull();
        capturedMetadata!["BulkRatesDownloadVersion"].Should().Be("3");
        capturedMetadata!["BulkRatesJobQueueId"].Should().Be(entry.JobQueueId.ToString());
    }

    // (8) Workbook rows exactly match the persisted snapshot

    [Fact]
    public async Task Download_WorkbookSheets_ContainExactlySnapshotRows()
    {
        var snapshotFec   = new[] { new FecStagingRow { TestCode = "T200", FecNewRate = 11.1m } } as IReadOnlyList<FecStagingRow>;
        var snapshotAgrup = new[] { new AgrupStagingRow { TestCode = "T200", Buyer = "VLA", Agrup = 2.0m } } as IReadOnlyList<AgrupStagingRow>;
        var entry = Entry(status: "Initiated");
        var repo  = RepoForDownload(entry, snapshotFec: snapshotFec, snapshotAgrup: snapshotAgrup);

        IReadOnlyList<ExcelSheetDefinition>? capturedSheets = null;
        var excel = Substitute.For<IExcelExportService>();
        excel.ExportToExcelMultiSheet(
                Arg.Do<IEnumerable<ExcelSheetDefinition>>(s => capturedSheets = s.ToList()),
                Arg.Any<IReadOnlyDictionary<string, string>>())
             .Returns([1]);
        var svc = CreateServiceWithExcel(repo, excel);

        await svc.DownloadFecTestDataAsync(entry.JobExecutionId);

        capturedSheets.Should().NotBeNull();
        capturedSheets!.Should().HaveCountGreaterThanOrEqualTo(2);
        // FEC sheet rows come from snapshot only (T200 present)
        var fecSheet = capturedSheets[0];
        fecSheet.Data.Cast<FecExportRow>().Should().Contain(r => r.TestCode == "T200");
    }

    // (9) Repeat download creates a higher version

    [Fact]
    public async Task Download_SecondCall_UsesHigherDownloadVersion()
    {
        var entry = Entry(status: "Initiated");

        // First call → version 1, second call → version 2
        var repoV1 = RepoForDownload(entry, nextVersion: 1);
        var svcV1  = CreateServiceWithExcel(repoV1);
        await svcV1.DownloadFecTestDataAsync(entry.JobExecutionId);
        await repoV1.Received(1).CreateDownloadSnapshotAsync(entry.JobQueueId, 1, Arg.Any<IReadOnlyList<FecStagingRow>>(), Arg.Any<IReadOnlyList<AgrupStagingRow>>(), Arg.Any<CancellationToken>());

        var repoV2 = RepoForDownload(entry, nextVersion: 2);
        var svcV2  = CreateServiceWithExcel(repoV2);
        await svcV2.DownloadFecTestDataAsync(entry.JobExecutionId);
        await repoV2.Received(1).CreateDownloadSnapshotAsync(entry.JobQueueId, 2, Arg.Any<IReadOnlyList<FecStagingRow>>(), Arg.Any<IReadOnlyList<AgrupStagingRow>>(), Arg.Any<CancellationToken>());
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a minimal BulkTestRatesUpdate xlsx in memory. When <paramref name="downloadVersion"/>
    /// is supplied, embeds the VeryHidden protected-metadata sheet (DR-VAL-03) carrying that
    /// version, mirroring what DownloadFecTestDataAsync writes; omit it to simulate a workbook
    /// that never went through the download flow.
    /// </summary>
    private static byte[] BuildMinimalXlsx(int? downloadVersion = null)
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

        if (downloadVersion is not null)
        {
            var meta = wb.Worksheets.Add(Apha.Common.Utilities.ExcelExport.ExcelExportService.ProtectedMetadataSheetName);
            meta.Cell(1, 1).Value = "BulkRatesJobQueueId";
            meta.Cell(1, 2).Value = QueueId.ToString();
            meta.Cell(2, 1).Value = "BulkRatesDownloadVersion";
            meta.Cell(2, 2).Value = downloadVersion.Value.ToString();
            meta.Visibility = ClosedXML.Excel.XLWorksheetVisibility.VeryHidden;
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
