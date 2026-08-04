using Apha.Common.BulkRates.Validation;
using Apha.Common.BulkRates.Validation.StaffAnimal;
using FluentAssertions;

namespace Apha.Common.UnitTests.BulkRates.Validation.StaffAnimal
{
    public class StaffAnimalValidationServiceTests
    {
        private readonly StaffAnimalValidationService _sut = new();
        private static readonly Guid JobQueueId = Guid.NewGuid();

        private static StaffAnimalValidationContext Context(
            IReadOnlyList<ValidationStaffRow>? staff = null,
            IReadOnlyList<ValidationAnimalRow>? animal = null,
            IReadOnlyDictionary<string, LiveStaffRow>? liveStaff = null,
            IReadOnlyDictionary<string, LiveAnimalRow>? liveAnimal = null)
            => new()
            {
                JobQueueId = JobQueueId,
                FpsYear = 2027,
                LiveStaffLookup = liveStaff ?? new Dictionary<string, LiveStaffRow>(),
                LiveAnimalLookup = liveAnimal ?? new Dictionary<string, LiveAnimalRow>(),
                StagedStaffRows = staff ?? [],
                StagedAnimalRows = animal ?? [],
            };

        private static ValidationStaffRow Staff(string pcGrade, decimal? payRate = 10, decimal? npr = 1, decimal? ohr = 1, int sourceRow = 2)
            => new() { PcGrade = pcGrade, PayRate = payRate, Npr = npr, Ohr = ohr, SourceRow = sourceRow };

        private static ValidationAnimalRow Animal(
            string animalType, decimal? dailyRate = 10, decimal? defraDailyRate = 5, bool? planByWeek = false,
            string? species = "Bovine", string? securityLevel = "Low", int sourceRow = 2)
            => new()
            {
                AnimalType = animalType, DailyRate = dailyRate, DefraDailyRate = defraDailyRate,
                PlanByWeek = planByWeek, Species = species, SecurityLevel = securityLevel, SourceRow = sourceRow,
            };

        // ── Staff ────────────────────────────────────────────────────────────────

        [Fact]
        public void Staff_BlankPcGrade_IsInvalid_MissingGrade()
        {
            var ctx = Context(staff: [Staff("")]);

            var result = _sut.Validate(ctx);

            var row = result.StaffResults.Should().ContainSingle().Which;
            row.Action.Should().Be(StaffAnimalCalculatedAction.Invalid);
            row.Errors.Should().ContainSingle(e => e.ValidationCode == "MISSING_GRADE" && e.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public void Staff_DuplicateGrade_IsInvalid()
        {
            var ctx = Context(staff: [Staff("G1", sourceRow: 2), Staff("g1", sourceRow: 3)]);

            var result = _sut.Validate(ctx);

            result.StaffResults.Should().HaveCount(2)
                .And.OnlyContain(r => r.Action == StaffAnimalCalculatedAction.Invalid
                    && r.Errors.Any(e => e.ValidationCode == "DUPLICATE_GRADE"));
        }

        [Theory]
        [InlineData(-1, 1, 1, "payrate")]
        [InlineData(1, -1, 1, "npr")]
        [InlineData(1, 1, -1, "ohr")]
        public void Staff_NegativeRate_IsInvalid(decimal payRate, decimal npr, decimal ohr, string expectedField)
        {
            var ctx = Context(staff: [Staff("G1", payRate, npr, ohr)]);

            var result = _sut.Validate(ctx);

            var row = result.StaffResults.Should().ContainSingle().Which;
            row.Action.Should().Be(StaffAnimalCalculatedAction.Invalid);
            row.Errors.Should().ContainSingle(e => e.ValidationCode == "NEGATIVE_RATE" && e.Field == expectedField);
        }

        [Fact]
        public void Staff_GradeNotFoundLive_IsNotFound_HardFailure()
        {
            var ctx = Context(staff: [Staff("G1")]);

            var result = _sut.Validate(ctx);

            var row = result.StaffResults.Should().ContainSingle().Which;
            row.Action.Should().Be(StaffAnimalCalculatedAction.NotFound);
            row.Source.Should().BeNull();
            row.Errors.Should().ContainSingle(e => e.ValidationCode == "GRADE_NOT_FOUND" && e.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public void Staff_SameAsLive_IsNoChange()
        {
            var ctx = Context(
                staff: [Staff("G1", 10, 1, 1)],
                liveStaff: new Dictionary<string, LiveStaffRow> { ["G1"] = new() { PcGrade = "G1", PayRate = 10, Npr = 1, Ohr = 1 } });

            var result = _sut.Validate(ctx);

            result.StaffResults.Single().Action.Should().Be(StaffAnimalCalculatedAction.NoChange);
        }

        [Fact]
        public void Staff_DifferentFromLive_IsUpdate()
        {
            var ctx = Context(
                staff: [Staff("G1", 15, 1, 1)],
                liveStaff: new Dictionary<string, LiveStaffRow> { ["G1"] = new() { PcGrade = "G1", PayRate = 10, Npr = 1, Ohr = 1 } });

            var result = _sut.Validate(ctx);

            var row = result.StaffResults.Single();
            row.Action.Should().Be(StaffAnimalCalculatedAction.Update);
            row.Effective!.PayRate.Should().Be(15);
            row.Source!.PayRate.Should().Be(10);
        }

        [Fact]
        public void Staff_ZeroIsOrdinaryValue_NotSpecialCased_ClassifiesAsUpdate()
        {
            // Gating decision #2: zero is a valid update, not a withdrawal/invalid — a live
            // non-zero PayRate going to an explicit 0 is an ordinary Update.
            var ctx = Context(
                staff: [Staff("G1", 0, 1, 1)],
                liveStaff: new Dictionary<string, LiveStaffRow> { ["G1"] = new() { PcGrade = "G1", PayRate = 10, Npr = 1, Ohr = 1 } });

            var result = _sut.Validate(ctx);

            var row = result.StaffResults.Single();
            row.Action.Should().Be(StaffAnimalCalculatedAction.Update);
            row.Errors.Should().BeEmpty();
            row.Effective!.PayRate.Should().Be(0);
        }

        [Fact]
        public void Staff_BlankUploadEquivalentToZero_MatchingLiveZero_IsNoChange()
        {
            var ctx = Context(
                staff: [Staff("G1", payRate: null, npr: 1, ohr: 1)],
                liveStaff: new Dictionary<string, LiveStaffRow> { ["G1"] = new() { PcGrade = "G1", PayRate = 0, Npr = 1, Ohr = 1 } });

            var result = _sut.Validate(ctx);

            result.StaffResults.Single().Action.Should().Be(StaffAnimalCalculatedAction.NoChange);
        }

        [Fact]
        public void Staff_BlankUploadEquivalentToZero_DifferentFromLiveNonZero_IsUpdate()
        {
            var ctx = Context(
                staff: [Staff("G1", payRate: null, npr: 1, ohr: 1)],
                liveStaff: new Dictionary<string, LiveStaffRow> { ["G1"] = new() { PcGrade = "G1", PayRate = 10, Npr = 1, Ohr = 1 } });

            var result = _sut.Validate(ctx);

            var row = result.StaffResults.Single();
            row.Action.Should().Be(StaffAnimalCalculatedAction.Update);
            row.Effective!.PayRate.Should().Be(0);
        }

        // ── Animal ───────────────────────────────────────────────────────────────

        [Fact]
        public void Animal_BlankAnimalType_IsInvalid_MissingAnimalType()
        {
            var ctx = Context(animal: [Animal("")]);

            var result = _sut.Validate(ctx);

            var row = result.AnimalResults.Should().ContainSingle().Which;
            row.Action.Should().Be(StaffAnimalCalculatedAction.Invalid);
            row.Errors.Should().ContainSingle(e => e.ValidationCode == "MISSING_ANIMAL_TYPE");
        }

        [Fact]
        public void Animal_DuplicateAnimalType_IsInvalid()
        {
            var ctx = Context(animal: [Animal("A1", sourceRow: 2), Animal("a1", sourceRow: 3)]);

            var result = _sut.Validate(ctx);

            result.AnimalResults.Should().HaveCount(2)
                .And.OnlyContain(r => r.Action == StaffAnimalCalculatedAction.Invalid
                    && r.Errors.Any(e => e.ValidationCode == "DUPLICATE_ANIMAL_TYPE"));
        }

        [Theory]
        [InlineData(-1, 5, "dailyrate")]
        [InlineData(10, -5, "defradailyrate")]
        public void Animal_NegativeRate_IsInvalid(decimal dailyRate, decimal defraDailyRate, string expectedField)
        {
            var ctx = Context(animal: [Animal("A1", dailyRate, defraDailyRate)]);

            var result = _sut.Validate(ctx);

            var row = result.AnimalResults.Should().ContainSingle().Which;
            row.Action.Should().Be(StaffAnimalCalculatedAction.Invalid);
            row.Errors.Should().ContainSingle(e => e.ValidationCode == "NEGATIVE_RATE" && e.Field == expectedField);
        }

        [Fact]
        public void Animal_TypeNotFoundLive_IsNotFound_HardFailure()
        {
            var ctx = Context(animal: [Animal("A1")]);

            var result = _sut.Validate(ctx);

            var row = result.AnimalResults.Should().ContainSingle().Which;
            row.Action.Should().Be(StaffAnimalCalculatedAction.NotFound);
            row.Source.Should().BeNull();
            row.Errors.Should().ContainSingle(e => e.ValidationCode == "ANIMAL_TYPE_NOT_FOUND" && e.Severity == ValidationSeverity.Error);
        }

        [Fact]
        public void Animal_SameAsLive_IsNoChange()
        {
            var ctx = Context(
                animal: [Animal("A1", 10, 5, false, "Bovine", "Low")],
                liveAnimal: new Dictionary<string, LiveAnimalRow>
                {
                    ["A1"] = new() { AnimalType = "A1", DailyRate = 10, DefraDailyRate = 5, PlanByWeek = false, Species = "Bovine", SecurityLevel = "Low" },
                });

            var result = _sut.Validate(ctx);

            result.AnimalResults.Single().Action.Should().Be(StaffAnimalCalculatedAction.NoChange);
        }

        [Fact]
        public void Animal_SpeciesDifferentCasingAndWhitespace_IsNoChange()
        {
            var ctx = Context(
                animal: [Animal("A1", 10, 5, false, species: "  bovine  ", securityLevel: "Low")],
                liveAnimal: new Dictionary<string, LiveAnimalRow>
                {
                    ["A1"] = new() { AnimalType = "A1", DailyRate = 10, DefraDailyRate = 5, PlanByWeek = false, Species = "Bovine", SecurityLevel = "Low" },
                });

            var result = _sut.Validate(ctx);

            result.AnimalResults.Single().Action.Should().Be(StaffAnimalCalculatedAction.NoChange);
        }

        [Fact]
        public void Animal_SpeciesChanged_IsUpdate()
        {
            var ctx = Context(
                animal: [Animal("A1", 10, 5, false, species: "Ovine", securityLevel: "Low")],
                liveAnimal: new Dictionary<string, LiveAnimalRow>
                {
                    ["A1"] = new() { AnimalType = "A1", DailyRate = 10, DefraDailyRate = 5, PlanByWeek = false, Species = "Bovine", SecurityLevel = "Low" },
                });

            var result = _sut.Validate(ctx);

            var row = result.AnimalResults.Single();
            row.Action.Should().Be(StaffAnimalCalculatedAction.Update);
            row.Effective!.Species.Should().Be("Ovine");
        }

        [Fact]
        public void Animal_BlankPlanByWeek_EquivalentToFalse_MatchingLiveFalse_IsNoChange()
        {
            var ctx = Context(
                animal: [Animal("A1", 10, 5, planByWeek: null)],
                liveAnimal: new Dictionary<string, LiveAnimalRow>
                {
                    ["A1"] = new() { AnimalType = "A1", DailyRate = 10, DefraDailyRate = 5, PlanByWeek = false, Species = "Bovine", SecurityLevel = "Low" },
                });

            var result = _sut.Validate(ctx);

            result.AnimalResults.Single().Action.Should().Be(StaffAnimalCalculatedAction.NoChange);
        }

        [Fact]
        public void Animal_BlankPlanByWeek_EquivalentToFalse_DifferentFromLiveTrue_IsUpdate()
        {
            var ctx = Context(
                animal: [Animal("A1", 10, 5, planByWeek: null)],
                liveAnimal: new Dictionary<string, LiveAnimalRow>
                {
                    ["A1"] = new() { AnimalType = "A1", DailyRate = 10, DefraDailyRate = 5, PlanByWeek = true, Species = "Bovine", SecurityLevel = "Low" },
                });

            var result = _sut.Validate(ctx);

            var row = result.AnimalResults.Single();
            row.Action.Should().Be(StaffAnimalCalculatedAction.Update);
            row.Effective!.PlanByWeek.Should().BeFalse();
        }

        [Fact]
        public void Animal_ZeroDailyRate_IsOrdinaryValue_NotSpecialCased_ClassifiesAsUpdate()
        {
            var ctx = Context(
                animal: [Animal("A1", dailyRate: 0, defraDailyRate: 5)],
                liveAnimal: new Dictionary<string, LiveAnimalRow>
                {
                    ["A1"] = new() { AnimalType = "A1", DailyRate = 10, DefraDailyRate = 5, PlanByWeek = false, Species = "Bovine", SecurityLevel = "Low" },
                });

            var result = _sut.Validate(ctx);

            var row = result.AnimalResults.Single();
            row.Action.Should().Be(StaffAnimalCalculatedAction.Update);
            row.Errors.Should().BeEmpty();
        }

        // ── validation_version ───────────────────────────────────────────────────

        [Fact]
        public void EveryResult_CarriesCurrentValidationVersion()
        {
            var ctx = Context(staff: [Staff("G1")], animal: [Animal("A1")]);

            var result = _sut.Validate(ctx);

            result.StaffResults.Single().ValidationVersion.Should().Be(StaffAnimalValidationVersion.Current);
            result.AnimalResults.Single().ValidationVersion.Should().Be(StaffAnimalValidationVersion.Current);
        }
    }
}
