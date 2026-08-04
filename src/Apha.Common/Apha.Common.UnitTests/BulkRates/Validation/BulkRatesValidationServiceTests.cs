using Apha.Common.BulkRates.Validation;
using FluentAssertions;

namespace Apha.Common.UnitTests.BulkRates.Validation
{
    public class BulkRatesValidationServiceTests
    {
        private readonly BulkRatesValidationService _sut = new();
        private static readonly Guid JobQueueId = Guid.NewGuid();

        private static ValidationContext Context(
            IReadOnlyList<ValidationFecRow>? fec = null,
            IReadOnlyList<ValidationAgrupRow>? agrup = null,
            IReadOnlyDictionary<string, LiveFecRow>? liveFec = null,
            IReadOnlyDictionary<(string, string), LiveAgrupRow>? liveAgrup = null,
            IReadOnlySet<string>? projectLookup = null,
            IReadOnlySet<(string, string)>? capabilityLookup = null,
            IReadOnlyList<DownloadedSnapshotKey>? frozenSnapshot = null,
            int? downloadVersion = null,
            bool includeWorkerOnlyChecks = false)
            => new()
            {
                JobQueueId = JobQueueId,
                FpsYear = 2027,
                DownloadVersion = downloadVersion,
                UploadVersion = 1,
                IncludeWorkerOnlyChecks = includeWorkerOnlyChecks,
                LiveFecLookup = liveFec ?? new Dictionary<string, LiveFecRow>(),
                LiveAgrupLookup = liveAgrup ?? new Dictionary<(string, string), LiveAgrupRow>(),
                ProjectLookup = projectLookup ?? new HashSet<string>(),
                CapabilityLookup = capabilityLookup ?? new HashSet<(string, string)>(),
                StagedFecRows = fec ?? [],
                StagedAgrupRows = agrup ?? [],
                FrozenSnapshot = frozenSnapshot ?? [],
            };

        private static ValidationFecRow Fec(string testCode, decimal? rate, int sourceRow = 2,
            string? item = "desc", string? shortDesc = "short", string? owner = "PT")
            => new() { TestCode = testCode, FecNewRate = rate, ItemDescription = item, ShortDescription = shortDesc, Owner = owner, SourceRow = sourceRow };

        private static ValidationAgrupRow Agrup(string testCode, string buyer, decimal? rate, int sourceRow = 2,
            string? projectBuyerCode = null, string? testBuyerCode = null, string? testBuyerWorkGroup = null, string? comments = null)
            => new()
            {
                TestCode = testCode, Buyer = buyer, AgrupNew = rate, SourceRow = sourceRow,
                ProjectBuyerCode = projectBuyerCode, TestBuyerCode = testBuyerCode, TestBuyerWorkGroup = testBuyerWorkGroup,
                Comments = comments,
            };

        // ── FEC: existing-row blank/zero → Zero-Rate Withdrawal (reconciliation §2.1) ──────────

        [Fact]
        public void ExistingFecRow_BlankRate_ClassifiesAsZeroRateWithdrawal_NotError()
        {
            var ctx = Context(
                fec: [Fec("TC001", null)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } });

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.ValidationCode == "MISSING_FEC_NEW_RATE");
            findings.Should().ContainSingle(f => f.ValidationCode == "ROW_CLASSIFIED")
                .Which.Should().BeEquivalentTo(new { CalculatedAction = ValidationCalculatedAction.ZeroRateWithdrawal, EffectiveNewRate = 0m },
                    o => o.ExcludingMissingMembers());
        }

        [Fact]
        public void ExistingFecRow_ExplicitZero_ClassifiesAsZeroRateWithdrawal()
        {
            var ctx = Context(
                fec: [Fec("TC001", 0)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } });

            var findings = _sut.Validate(ctx);

            findings.Single(f => f.ValidationCode == "ROW_CLASSIFIED").CalculatedAction.Should().Be(ValidationCalculatedAction.ZeroRateWithdrawal);
        }

        [Fact]
        public void ExistingFecRow_AlreadyZeroLiveRate_BlankUpload_ClassifiesAsNoChange_NotWithdrawal()
        {
            // The live rate is already 0 — re-uploading blank/zero isn't a fresh withdrawal,
            // nothing is actually changing.
            var ctx = Context(
                fec: [Fec("TC001", null)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 0, DefraUnitPrice = 0 } });

            var findings = _sut.Validate(ctx);

            findings.Single(f => f.ValidationCode == "ROW_CLASSIFIED").CalculatedAction.Should().Be(ValidationCalculatedAction.NoChange);
        }

        [Fact]
        public void NewFecRow_BlankRate_IsBlockingError()
        {
            var ctx = Context(fec: [Fec("TC999", null)]);

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "MISSING_FEC_NEW_RATE" && f.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public void ExistingFecRow_SameRateAsLive_ClassifiesAsNoChange()
        {
            var ctx = Context(
                fec: [Fec("TC001", 10)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } });

            var findings = _sut.Validate(ctx);

            findings.Single(f => f.ValidationCode == "ROW_CLASSIFIED").CalculatedAction.Should().Be(ValidationCalculatedAction.NoChange);
        }

        [Fact]
        public void ExistingFecRow_DifferentRate_ClassifiesAsUpdate()
        {
            var ctx = Context(
                fec: [Fec("TC001", 15)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } });

            var findings = _sut.Validate(ctx);

            var classification = findings.Single(f => f.ValidationCode == "ROW_CLASSIFIED");
            classification.CalculatedAction.Should().Be(ValidationCalculatedAction.Update);
            classification.EffectiveNewRate.Should().Be(15);
        }

        [Fact]
        public void NewFecRow_ValidData_ClassifiesAsInsert()
        {
            var ctx = Context(fec: [Fec("TC999", 5)]);

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.Severity == ValidationSeverity.Error);
            findings.Single(f => f.ValidationCode == "ROW_CLASSIFIED").CalculatedAction.Should().Be(ValidationCalculatedAction.Insert);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NewFecRow_MissingRequiredFields_RaisesMissingForInsert(string? blank)
        {
            var ctx = Context(fec: [Fec("TC999", 5, item: blank, shortDesc: blank, owner: blank)]);

            var findings = _sut.Validate(ctx);

            findings.Count(f => f.ValidationCode == "MISSING_FOR_INSERT").Should().Be(3);
        }

        [Fact]
        public void FecRow_NegativeRate_IsBlockingError_RegardlessOfNewOrExisting()
        {
            var ctx = Context(
                fec: [Fec("TC001", -5)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } });

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "NEGATIVE_RATE" && f.Sheet == "FEC");
            findings.Should().NotContain(f => f.ValidationCode == "ROW_CLASSIFIED");
        }

        [Fact]
        public void DuplicateFecTestCode_RaisesError()
        {
            var ctx = Context(fec: [Fec("TC001", 5, sourceRow: 2), Fec("tc001", 6, sourceRow: 3)]);

            var findings = _sut.Validate(ctx);

            findings.Count(f => f.ValidationCode == "DUPLICATE_TEST_CODE").Should().Be(2);
        }

        [Fact]
        public void ExistingFecRow_ItemDescriptionSupplied_RaisesIgnoredOnUpdateWarning()
        {
            var ctx = Context(
                fec: [Fec("TC001", 15, item: "changed")],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } });

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "IGNORED_ON_UPDATE" && f.Severity == ValidationSeverity.Warning);
        }

        // ── AGRUP: existing-row blank/zero → Zero-Rate Withdrawal (reconciliation §2.2) ──────────

        [Fact]
        public void ExistingAgrupRow_BlankRate_ClassifiesAsZeroRateWithdrawal_NotUnchanged()
        {
            var ctx = Context(
                fec: [Fec("TC001", 10)],
                agrup: [Agrup("TC001", "B001", null)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow> { [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 10 } });

            var findings = _sut.Validate(ctx);

            findings.Where(f => f.Sheet == "AGRUP" && f.ValidationCode == "ROW_CLASSIFIED")
                .Should().ContainSingle().Which.CalculatedAction.Should().Be(ValidationCalculatedAction.ZeroRateWithdrawal);
        }

        [Fact]
        public void ExistingAgrupRow_AlreadyZeroLiveRate_BlankUpload_ClassifiesAsNoChange_NotWithdrawal()
        {
            var ctx = Context(
                fec: [Fec("TC001", 10)],
                agrup: [Agrup("TC001", "B001", null)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow> { [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 0 } });

            var findings = _sut.Validate(ctx);

            findings.Where(f => f.Sheet == "AGRUP" && f.ValidationCode == "ROW_CLASSIFIED")
                .Should().ContainSingle().Which.CalculatedAction.Should().Be(ValidationCalculatedAction.NoChange);
        }

        [Fact]
        public void NewAgrupRow_ZeroRate_IsBlocked_BC01()
        {
            var ctx = Context(
                fec: [Fec("TC001", 10)],
                agrup: [Agrup("TC001", "B999", 0, projectBuyerCode: "PRJ001")],
                projectLookup: new HashSet<string> { "PRJ001" });

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "NEW_AGRUP_ZERO_RATE_BLOCKED");
        }

        [Fact]
        public void AgrupRow_UnknownTestCode_RaisesTestCodeNotFound()
        {
            var ctx = Context(agrup: [Agrup("UNKNOWN", "B001", 5, projectBuyerCode: "PRJ001")], projectLookup: new HashSet<string> { "PRJ001" });

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "TEST_CODE_NOT_FOUND");
        }

        [Fact]
        public void AgrupRow_TestCodeInSameUploadFecSheet_DoesNotRaiseTestCodeNotFound()
        {
            var ctx = Context(
                fec: [Fec("TC999", 5)],
                agrup: [Agrup("TC999", "B001", 5, projectBuyerCode: "PRJ001")],
                projectLookup: new HashSet<string> { "PRJ001" });

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.ValidationCode == "TEST_CODE_NOT_FOUND");
        }

        // ── AGRUP routing fields ────────────────────────────────────────────────

        [Fact]
        public void NewAgrupRow_NoRoutingFieldSupplied_RaisesMissingRoutingField()
        {
            var ctx = Context(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 5)]);

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "MISSING_ROUTING_FIELD");
        }

        [Fact]
        public void NewAgrupRow_InvalidProjectBuyerCode_RaisesError()
        {
            var ctx = Context(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 5, projectBuyerCode: "BOGUS")]);

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "INVALID_PROJECT_BUYER_CODE");
        }

        [Fact]
        public void NewAgrupRow_ValidProjectBuyerCode_NoRoutingError()
        {
            var ctx = Context(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 5, projectBuyerCode: "PRJ001")],
                projectLookup: new HashSet<string> { "PRJ001" });

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.ValidationCode == "MISSING_ROUTING_FIELD" || f.ValidationCode == "INVALID_PROJECT_BUYER_CODE");
        }

        [Fact]
        public void NewAgrupRow_InvalidTestBuyerWorkGroup_RaisesError()
        {
            var ctx = Context(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 5, testBuyerWorkGroup: "WG-BOGUS")]);

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "INVALID_TEST_BUYER_WORKGROUP");
        }

        [Fact]
        public void NewAgrupRow_ValidTestBuyerWorkGroup_NoRoutingError()
        {
            var ctx = Context(fec: [Fec("TC001", 10)], agrup: [Agrup("TC001", "B999", 5, testBuyerWorkGroup: "WG1")],
                capabilityLookup: new HashSet<(string, string)> { ("TC001", "WG1") });

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.ValidationCode == "MISSING_ROUTING_FIELD" || f.ValidationCode == "INVALID_TEST_BUYER_WORKGROUP");
        }

        [Fact]
        public void NewAgrupRow_BothRoutingFieldsPopulated_IsPermitted_BC03()
        {
            var ctx = Context(fec: [Fec("TC001", 10)],
                agrup: [Agrup("TC001", "B999", 5, projectBuyerCode: "PRJ001", testBuyerWorkGroup: "WG1")],
                projectLookup: new HashSet<string> { "PRJ001" },
                capabilityLookup: new HashSet<(string, string)> { ("TC001", "WG1") });

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.Severity == ValidationSeverity.Error);
        }

        // ── AGRUP existing-row routing immutability (reconciliation §2.5) ──────────────

        [Fact]
        public void ExistingAgrupRow_ChangedProjectBuyerCode_RaisesRoutingFieldChanged()
        {
            var ctx = Context(
                fec: [Fec("TC001", 10)],
                agrup: [Agrup("TC001", "B001", 5, projectBuyerCode: "PRJ002")],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow>
                {
                    [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 5, ProjectBuyerCode = "PRJ001" }
                });

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "ROUTING_FIELD_CHANGED" && f.Field == "projectbuyercode");
        }

        [Fact]
        public void ExistingAgrupRow_SameProjectBuyerCode_DifferentCase_IsNotChanged_CitextSemantics()
        {
            var ctx = Context(
                fec: [Fec("TC001", 10)],
                agrup: [Agrup("TC001", "B001", 5, projectBuyerCode: "prj001")],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow>
                {
                    [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 5, ProjectBuyerCode = "PRJ001" }
                });

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.ValidationCode == "ROUTING_FIELD_CHANGED");
        }

        [Fact]
        public void ExistingAgrupRow_UnchangedRoutingFields_NoRoutingCapabilityRevalidation()
        {
            // Existing rows aren't re-checked against ProjectLookup/CapabilityLookup at all —
            // only new rows are (immutability, not re-validation).
            var ctx = Context(
                fec: [Fec("TC001", 10)],
                agrup: [Agrup("TC001", "B001", 5, projectBuyerCode: "PRJ001")],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow>
                {
                    [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 5, ProjectBuyerCode = "PRJ001" }
                },
                projectLookup: new HashSet<string>()); // deliberately empty/stale — must not matter for an existing row

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.ValidationCode == "INVALID_PROJECT_BUYER_CODE" || f.ValidationCode == "MISSING_ROUTING_FIELD");
        }

        // ── FEC-withdrawal / AGRUP conflict (interim BC-05 safety net) ──────────────

        [Fact]
        public void WithdrawnFecTestCode_StagedPositiveAgrupRow_RaisesConflictError()
        {
            var ctx = Context(
                fec: [Fec("TC001", null)], // withdrawal
                agrup: [Agrup("TC001", "B001", 5)], // still positive in the upload
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow> { [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 5 } });

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "AGRUP_POSITIVE_FOR_WITHDRAWN_FEC");
        }

        [Fact]
        public void WithdrawnFecTestCode_StagedZeroAgrupRow_NoConflictError()
        {
            var ctx = Context(
                fec: [Fec("TC001", null)],
                agrup: [Agrup("TC001", "B001", 0)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow> { [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 5 } });

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.ValidationCode == "AGRUP_POSITIVE_FOR_WITHDRAWN_FEC");
        }

        [Fact]
        public void WithdrawnFecTestCode_LiveAgrupRowNotInSnapshot_RaisesWorkerInterimRuleFinding_WhenWorkerOnlyChecksIncluded()
        {
            // §5.2: a live AGRUP row created/linked after download, so the staged-vs-snapshot check
            // could never have caught it — this is the worker-side interim check, opted into via
            // IncludeWorkerOnlyChecks (only the worker's revalidation pass should set this).
            var ctx = Context(
                fec: [Fec("TC001", null)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow> { [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 5 } },
                frozenSnapshot: [new DownloadedSnapshotKey { Sheet = "FEC", TestCode = "TC001", SourceRate = 10 }],
                downloadVersion: 1,
                includeWorkerOnlyChecks: true);

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "LIVE_AGRUP_POSITIVE_FOR_WITHDRAWN_FEC" && f.IsRequestLevel);
        }

        [Fact]
        public void WithdrawnFecTestCode_LiveAgrupRowNotInSnapshot_NotRaised_WhenWorkerOnlyChecksExcluded()
        {
            // A live row created after download "cannot be caught here —
            // that gap is the worker's job." At API/release time (IncludeWorkerOnlyChecks=false,
            // the default), this must never surface — otherwise release would be blocked on a
            // row nobody reviewing the release could possibly have seen.
            var ctx = Context(
                fec: [Fec("TC001", null)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow> { [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 5 } },
                frozenSnapshot: [new DownloadedSnapshotKey { Sheet = "FEC", TestCode = "TC001", SourceRate = 10 }],
                downloadVersion: 1);

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.ValidationCode == "LIVE_AGRUP_POSITIVE_FOR_WITHDRAWN_FEC");
        }

        [Fact]
        public void WithdrawnFecTestCode_LiveAgrupRowInSnapshot_OnlyStagedCheckApplies_NoDoubleReport()
        {
            var ctx = Context(
                fec: [Fec("TC001", null)],
                agrup: [Agrup("TC001", "B001", 5)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow> { [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 5 } },
                frozenSnapshot:
                [
                    new DownloadedSnapshotKey { Sheet = "FEC", TestCode = "TC001", SourceRate = 10 },
                    new DownloadedSnapshotKey { Sheet = "AGRUP", TestCode = "TC001", Buyer = "B001", SourceRate = 5 },
                ],
                downloadVersion: 1,
                includeWorkerOnlyChecks: true);

            var findings = _sut.Validate(ctx);

            findings.Count(f => f.ValidationCode == "AGRUP_POSITIVE_FOR_WITHDRAWN_FEC" || f.ValidationCode == "LIVE_AGRUP_POSITIVE_FOR_WITHDRAWN_FEC")
                .Should().Be(1);
            findings.Should().ContainSingle(f => f.ValidationCode == "AGRUP_POSITIVE_FOR_WITHDRAWN_FEC");
        }

        // ── Downloaded-snapshot preservation (reconciliation §2.6) ──────────────────────

        [Fact]
        public void MissingDownloadedFecKey_RaisesRequestLevelError()
        {
            var ctx = Context(
                fec: [], // TC001 was downloaded but not re-uploaded
                frozenSnapshot: [new DownloadedSnapshotKey { Sheet = "FEC", TestCode = "TC001", SourceRate = 10 }],
                downloadVersion: 1);

            var findings = _sut.Validate(ctx);

            var finding = findings.Should().ContainSingle(f => f.ValidationCode == "MISSING_DOWNLOADED_KEY" && f.Sheet == "FEC").Subject;
            finding.IsRequestLevel.Should().BeTrue();
            finding.SourceRow.Should().BeNull();
        }

        [Fact]
        public void MissingDownloadedAgrupKey_RaisesRequestLevelError()
        {
            var ctx = Context(
                frozenSnapshot: [new DownloadedSnapshotKey { Sheet = "AGRUP", TestCode = "TC001", Buyer = "B001", SourceRate = 5 }],
                downloadVersion: 1);

            var findings = _sut.Validate(ctx);

            findings.Should().ContainSingle(f => f.ValidationCode == "MISSING_DOWNLOADED_KEY" && f.Sheet == "AGRUP" && f.IsRequestLevel);
        }

        [Fact]
        public void DownloadedKeyStillPresentInUpload_NoMissingKeyError()
        {
            var ctx = Context(
                fec: [Fec("TC001", 12)],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                frozenSnapshot: [new DownloadedSnapshotKey { Sheet = "FEC", TestCode = "TC001", SourceRate = 10 }],
                downloadVersion: 1);

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.ValidationCode == "MISSING_DOWNLOADED_KEY");
        }

        [Fact]
        public void NoDownloadVersion_SkipsSnapshotPreservationCheck()
        {
            // A request with no download yet has nothing to compare against.
            var ctx = Context(downloadVersion: null, frozenSnapshot: []);

            var findings = _sut.Validate(ctx);

            findings.Should().NotContain(f => f.ValidationCode == "MISSING_DOWNLOADED_KEY");
        }

        // ── Determinism (§3.2) ──────────────────────────────────────────────────

        [Fact]
        public void Validate_IsDeterministic_SameContextProducesSameFindings()
        {
            var ctx = Context(
                fec: [Fec("TC001", null), Fec("TC999", 5)],
                agrup: [Agrup("TC001", "B001", 5), Agrup("TC999", "B999", 3, projectBuyerCode: "PRJ001")],
                liveFec: new Dictionary<string, LiveFecRow> { ["TC001"] = new() { TestCode = "TC001", UnitPriceVla = 10, DefraUnitPrice = 10 } },
                liveAgrup: new Dictionary<(string, string), LiveAgrupRow> { [("TC001", "B001")] = new() { TestCode = "TC001", Buyer = "B001", UnitPrice = 5 } },
                projectLookup: new HashSet<string> { "PRJ001" });

            var first = _sut.Validate(ctx);
            var second = _sut.Validate(ctx);

            second.Should().BeEquivalentTo(first);
        }
    }
}
