namespace Apha.Common.Utilities.Storage
{
    public interface IS3StorageService
    {
        Task<S3UploadResult> UploadFileAsync(
            Stream fileStream,
            string bucketName,
            string folderPath,
            string fileName,
            string? contentType = null,
            CancellationToken cancellationToken = default);
    }
}
