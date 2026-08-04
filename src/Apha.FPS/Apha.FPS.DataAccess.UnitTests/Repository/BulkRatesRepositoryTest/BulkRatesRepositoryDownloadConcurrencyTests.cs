using Apha.Common.Constants;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Npgsql;

namespace Apha.FPS.DataAccess.UnitTests.Repository.BulkRatesRepositoryTest
{
    /// <summary>
    /// A concurrency-safety requirement: PostgreSQL-backed
    /// integration test proving <see cref="BulkRatesRepository.MarkDownloadReadyAsync"/> never
    /// lets an older, late-finishing download version regress
    /// <c>fps.job_queue.active_download_version</c> once a newer version has already activated —
    /// it must never be allowed to overwrite active_download_version back to itself. The
    /// race is simulated deterministically via out-of-order calls (the property under test is the
    /// WHERE-guarded UPDATE itself, not thread scheduling). <see cref="BulkRatesRepository"/> is
    /// shared by FEC/AGRUP and Staff/Animal alike — exercised here via a Staff request, but the fix
    /// and this test cover every job type that calls MarkDownloadReadyAsync.
    ///
    /// Soft-skips (no assertions run, test still passes) when the local integration Postgres is
    /// unreachable. CI safety does not rely on that self-skip — this class is named in
    /// fps-api-ci.yaml's dataaccess_test_filter so it never runs where no Postgres is reachable,
    /// matching this repo's established Apha.BatchJobs.UnitTests convention (explicit, auditable
    /// exclusion, not reliance on runtime skip logic alone).
    /// </summary>
    public sealed class BulkRatesRepositoryDownloadConcurrencyTests : IAsyncLifetime
    {
        private const string DefaultConnectionString =
            "Host=localhost;Port=5432;Database=batch_jobs_foundation_db_cloud;Username=postgres;Password=admin123;SSL Mode=Disable";
        private readonly string _connectionString;
        private bool _dbAvailable;
        private readonly List<Guid> _createdJobQueueIds = new();

        public BulkRatesRepositoryDownloadConcurrencyTests()
        {
            _connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
                ?? DefaultConnectionString;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                _dbAvailable = true;
            }
            catch
            {
                _dbAvailable = false;
            }
        }

        public async Task DisposeAsync()
        {
            if (!_dbAvailable || _createdJobQueueIds.Count == 0) return;

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            foreach (var jobQueueId in _createdJobQueueIds)
            {
                await using var cmd = conn.CreateCommand();
                // job_queue.active_download_version and bulk_rates_download.jobqueueid form a
                // circular FK pair, so the cycle must be broken (null the pointer) before either
                // side can be deleted.
                cmd.CommandText = @"
                    UPDATE fps.job_queue SET active_download_version = NULL WHERE jobqueueid = @id;
                    DELETE FROM fps.bulk_rates_staff_downloaded_key WHERE jobqueueid = @id;
                    DELETE FROM fps.bulk_rates_download WHERE jobqueueid = @id;
                    DELETE FROM fps.job_queue WHERE jobqueueid = @id;";
                cmd.Parameters.AddWithValue("id", jobQueueId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private BulkRatesRepository CreateRepository()
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            var options = new DbContextOptionsBuilder<FpsDbContext>().UseNpgsql(_connectionString).Options;
            var context = new FpsDbContext(options, requestContext);
            return new BulkRatesRepository(context, NullLogger<BulkRatesRepository>.Instance);
        }

        private async Task<Guid> CreateStaffRequestAsync(BulkRatesRepository repo, int fpsYear)
        {
            var jobId = await repo.GetJobIdByNameAsync(BulkRatesJobNames.Staff)
                ?? throw new InvalidOperationException($"fps.job_master has no '{BulkRatesJobNames.Staff}' row.");
            var statusId = await repo.GetStatusIdByNameAsync(jobId, "Initiated")
                ?? throw new InvalidOperationException($"fps.job_status has no 'Initiated' row for '{BulkRatesJobNames.Staff}'.");

            var jobQueueId = Guid.NewGuid();
            await repo.CreateRequestAsync(
                jobQueueId, Guid.NewGuid(), jobId, statusId,
                "integration-test-requester", DateTime.UtcNow, fpsYear);
            _createdJobQueueIds.Add(jobQueueId);
            return jobQueueId;
        }

        private async Task<int?> ReadActiveDownloadVersionAsync(Guid jobQueueId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT active_download_version FROM fps.job_queue WHERE jobqueueid = @id;";
            cmd.Parameters.AddWithValue("id", jobQueueId);
            var result = await cmd.ExecuteScalarAsync();
            return result is null or DBNull ? null : Convert.ToInt32(result);
        }

        private async Task<string> ReadDownloadStatusAsync(Guid jobQueueId, int downloadVersion)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT status FROM fps.bulk_rates_download
                WHERE jobqueueid = @id AND download_version = @version;";
            cmd.Parameters.AddWithValue("id", jobQueueId);
            cmd.Parameters.AddWithValue("version", downloadVersion);
            return (string)(await cmd.ExecuteScalarAsync())!;
        }

        [Fact]
        public async Task MarkDownloadReadyAsync_OlderVersionFinishesAfterNewerIsActive_DoesNotRegressActiveVersion()
        {
            if (!_dbAvailable) return;

            var repo = CreateRepository();
            var jobQueueId = await CreateStaffRequestAsync(repo, fpsYear: 2024);

            // Two downloads both start Generating for the same request (e.g. one slow initial
            // load, one quick retry from a second tab) — version 1 starts first, version 2 second.
            var v1 = await repo.GetNextDownloadVersionAsync(jobQueueId);
            await repo.CreateStaffDownloadSnapshotAsync(jobQueueId, v1, Array.Empty<StaffStagingRow>());

            var v2 = await repo.GetNextDownloadVersionAsync(jobQueueId);
            await repo.CreateStaffDownloadSnapshotAsync(jobQueueId, v2, Array.Empty<StaffStagingRow>());

            // v2 (the newer, faster one) finishes and activates first.
            await repo.MarkDownloadReadyAsync(jobQueueId, v2);
            Assert.Equal(v2, await ReadActiveDownloadVersionAsync(jobQueueId));

            // v1 (the older, slower one) finishes late — must NOT regress the active pointer,
            // even though it still marks its own header row
            // Ready (an accurate historical record for that version).
            await repo.MarkDownloadReadyAsync(jobQueueId, v1);
            Assert.Equal(v2, await ReadActiveDownloadVersionAsync(jobQueueId));
            Assert.Equal("Ready", await ReadDownloadStatusAsync(jobQueueId, v1));
            Assert.Equal("Ready", await ReadDownloadStatusAsync(jobQueueId, v2));
        }

        [Fact]
        public async Task MarkDownloadReadyAsync_InOrder_ActivatesEachVersionInTurn()
        {
            if (!_dbAvailable) return;

            var repo = CreateRepository();
            var jobQueueId = await CreateStaffRequestAsync(repo, fpsYear: 2024);

            var v1 = await repo.GetNextDownloadVersionAsync(jobQueueId);
            await repo.CreateStaffDownloadSnapshotAsync(jobQueueId, v1, Array.Empty<StaffStagingRow>());
            await repo.MarkDownloadReadyAsync(jobQueueId, v1);
            Assert.Equal(v1, await ReadActiveDownloadVersionAsync(jobQueueId));

            var v2 = await repo.GetNextDownloadVersionAsync(jobQueueId);
            await repo.CreateStaffDownloadSnapshotAsync(jobQueueId, v2, Array.Empty<StaffStagingRow>());
            await repo.MarkDownloadReadyAsync(jobQueueId, v2);
            Assert.Equal(v2, await ReadActiveDownloadVersionAsync(jobQueueId));
        }
    }
}
