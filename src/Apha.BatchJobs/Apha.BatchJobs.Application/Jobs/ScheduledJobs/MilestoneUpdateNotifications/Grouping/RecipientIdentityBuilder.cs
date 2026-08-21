using System.Security.Cryptography;
using System.Text;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Grouping;

/// <summary>
/// Implementation of <see cref="IRecipientIdentityBuilder"/>. Grouping is always the
/// MNumber+Name+Email composite — never MNumber alone, since tblprojectmanager's PK is
/// the name column, not mnumber, so nothing enforces mnumber uniqueness; two rows
/// sharing an MNumber but differing in email must legacy-group as separate recipients
/// (plan section 9.1).
/// </summary>
public sealed class RecipientIdentityBuilder : IRecipientIdentityBuilder
{
    private const string NoMNumberSentinel = "<NO_MNUMBER>";
    private const string NoEmailSentinel = "<NO_EMAIL>";
    private const string NoNameSentinel = "<NO_NAME>"; // defensive — should be unreachable given the inner join, plan section 7
    private const char Separator = '|';

    /// <inheritdoc />
    public string BuildRecipientId(string? mNumber, string? projectManager, string? email)
    {
        var normalizedMNumber = Normalize(mNumber) ?? NoMNumberSentinel;
        var normalizedName = Normalize(projectManager) ?? NoNameSentinel;
        var normalizedEmail = Normalize(email) ?? NoEmailSentinel;

        var composite = string.Join(Separator, normalizedMNumber, normalizedName, normalizedEmail);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(composite));
        return Convert.ToHexString(hash);
    }

    /// <inheritdoc />
    public string? BuildDurablePersonId(string? mNumber) => Normalize(mNumber);

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim().ToUpperInvariant();
    }
}
