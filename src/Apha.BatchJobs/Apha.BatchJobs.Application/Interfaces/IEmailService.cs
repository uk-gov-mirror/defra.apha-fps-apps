using Apha.BatchJobs.Domain.Entities.Email;

namespace Apha.BatchJobs.Application.Interfaces;

public interface IEmailService
{
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
