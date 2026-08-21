namespace Apha.BatchJobs.Domain.Entities.Email;

/// <summary>
/// Outcome of one <c>IEmailService.SendAsync</c> call (plan section 10.1, section 11.2).
/// A failed send does not throw — the caller decides how to record/continue, per
/// spec section 14's "one recipient failure must not stop the remaining recipients".
/// </summary>
public sealed record EmailSendResult(bool Succeeded, string? FailureMessage = null)
{
    public static EmailSendResult Sent() => new(true);

    public static EmailSendResult Failed(string reason) => new(false, reason);
}
