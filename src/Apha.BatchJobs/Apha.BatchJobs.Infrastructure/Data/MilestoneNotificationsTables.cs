namespace Apha.BatchJobs.Infrastructure.Data;

// MilestoneUpdateNotifications read-only source table/view models.
// mabarchive.vlatestmonthyear, mabarchive.vprojectreports_pmmilestoneemail, and
// mabarchive.tbl_settings are queried read-only here — none of them are owned by
// BatchJobs (see monthly-business-notifications-implementation-plan.md section 5),
// so no loader/writer exists for any of these types.

/// <summary>
/// Keyless read model for mabarchive.vlatestmonthyear — the reporting-year fallback
/// source when the authoritative candidate query (vprojectreports_pmmilestoneemail)
/// returns zero rows. See plan section 6.1.
/// </summary>
internal sealed class MaVLatestMonthYear
{
    public int Year { get; set; }
    public int? LatestMonthReleased { get; set; }
    public string? Period { get; set; }
}

/// <summary>
/// Keyless read model for mabarchive.vprojectreports_pmmilestoneemail — the single
/// authoritative query for milestone notification eligibility (plan section 7).
/// Already embeds the full legacy chain (RadTrackProg, project status, project group
/// exclusion, manager resolution, milestone existence/due-year matching); this worker
/// must not re-derive any of that filtering.
/// </summary>
internal sealed class MaVProjectReportsPmMilestoneEmail
{
    public string? HLink { get; set; }
    public string? ProjectManager { get; set; }
    public string? MNumber { get; set; }
    public required string ParentProject { get; set; }
    public string? Email { get; set; }
    public string? EditLink { get; set; }
    public int Year { get; set; }
    public bool Disable { get; set; }
}

/// <summary>
/// Keyless read model for mabarchive.tbl_settings — used by the section 8.1 preflight
/// to confirm PIMS_Project_Report_Name/PIMS_Project_Current_Root/PIMS_Project_Edit_Link
/// each resolve to exactly one row before the authoritative query runs (a missing row
/// collapses that view to zero rows silently, for every project).
/// </summary>
internal sealed class MaTblSettings
{
    public required string Id { get; set; }
    public string? Setting { get; set; }
    public string? Notes { get; set; }
    public string? TestSetting { get; set; }
    public bool? UserUpdateable { get; set; }
}
