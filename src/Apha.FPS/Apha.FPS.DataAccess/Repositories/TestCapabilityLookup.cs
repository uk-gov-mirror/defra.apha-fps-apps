using Apha.Common.BulkRates.Validation;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Implementation: queries fps.tlkptestcapability directly on (testcode,
    /// workgroup) as two real columns. Deliberately does not reference Apha.PACT — fps.tlkptestcapability
    /// is the real reference table behind the legacy trigger's capability check (distinct from
    /// fps.tlkptestreqmt, which is what Apha.PACT's own ExistsByTestBuyerCodeAsync actually
    /// queries today), and Bulk Rates already reads other PACT-conceptually-owned live tables
    /// (fps.testorproduct/fps.tlkptestreqmt) the same way, via its own repository, not by
    /// referencing Apha.PACT's project/DbContext.
    /// </summary>
    public sealed class TestCapabilityLookup : ITestCapabilityLookup
    {
        private readonly FpsDbContext _dbContext;

        public TestCapabilityLookup(FpsDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<bool> ExistsAsync(string testCode, string workGroup, int fpsYear, CancellationToken ct = default)
        {
            await _dbContext.Database.OpenConnectionAsync(ct);
            var conn = (NpgsqlConnection)_dbContext.Database.GetDbConnection();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 1 FROM fps.tlkptestcapability
                WHERE fpsyear = @fpsyear AND testcode = @testcode AND workgroup = @workgroup;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);
            cmd.Parameters.AddWithValue("testcode", testCode);
            cmd.Parameters.AddWithValue("workgroup", workGroup);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result is not null;
        }
    }
}
