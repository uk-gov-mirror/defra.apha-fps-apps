namespace Apha.BatchJobs.Domain.Configuration;

/// <summary>
/// Configuration settings for the MABArchive scheduled job.
/// </summary>
public class MabArchiveSettings
{
    /// <summary>
    /// Lock timeout in seconds. Default: 3600 (1 hour).
    /// </summary>
    public int LockTimeoutSeconds { get; set; } = 0;

    /// <summary>
    /// Enforces year-aware joins and source view contracts for totals rebuild.
    /// When true, totals source views must expose fpsyear and joins must include fpsyear.
    /// </summary>
    public bool StrictYearIsolation { get; set; } = true;

    /// <summary>
    /// CR-028: identity used to reproduce the legacy sp_AddMY_Staff ProfitCentre authorization filter
    /// (originally WGE.WorkGroupGrade IN ... WHERE UPC.[User_ID] IN (SELECT U.[User_ID] FROM tblUsers U
    /// WHERE U.UserName = USER_NAME(1))). Legacy resolved this from the SQL login executing the batch job;
    /// this setting is the closest equivalent for a headless worker. Defaults to "dbo" based on local-DB
    /// investigation (2026-07-07): "dbo" is the only fps.tblusers row with access to every profit centre in
    /// fps.tbluser_profitcentre (103/103) and the only non-named-individual account, making it the most
    /// plausible historical batch identity. Not confirmed against the actual legacy SQL Agent job step
    /// identity or against the cloud database. If this identity is not found in fps.tblusers, or resolves to
    /// zero permitted profit centres, MyStaffLoader logs a warning and falls back to loading all staff
    /// (matching pre-CR-028 behavior) rather than silently archiving zero rows.
    /// </summary>
    public string StaffProfitCentreAuthorizationUserName { get; set; } = "dbo";
}
