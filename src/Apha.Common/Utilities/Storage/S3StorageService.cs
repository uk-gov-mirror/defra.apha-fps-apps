using Amazon.S3;
using Amazon.S3.Model;

namespace Apha.Common.Utilities.Storage
{
    public class S3StorageService : IS3StorageService
    {
        private readonly IAmazonS3 _amazonS3;

        public S3StorageService(IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
        }

        public async Task<S3UploadResult> UploadFileAsync(
            Stream fileStream,
            string bucketName,
            string folderPath,
            string fileName,
            string? contentType = null,
            CancellationToken cancellationToken = default)
        {
            if (fileStream == null)
                return S3UploadResult.FailureResponse("S3_INVALID_STREAM", "The file stream cannot be null.");

            if (string.IsNullOrWhiteSpace(bucketName))
                return S3UploadResult.FailureResponse("S3_INVALID_BUCKET", "The S3 bucket name is required.");

            if (string.IsNullOrWhiteSpace(fileName))
                return S3UploadResult.FailureResponse("S3_INVALID_FILENAME", "The file name is required for S3 upload.");

            try
            {
                try
                {
                    await _amazonS3.GetBucketAclAsync(new GetBucketAclRequest { BucketName = bucketName }, cancellationToken);
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound || string.Equals(ex.ErrorCode, "NoSuchBucket", StringComparison.OrdinalIgnoreCase))
                {
                    return S3UploadResult.FailureResponse(
                        "S3_BUCKET_NOT_FOUND",
                        $"The S3 bucket '{bucketName}' does not exist. File was not stored for audit.");
                }

                var normalizedFolder = NormalizeFolderPath(folderPath);

                if (fileStream.CanSeek)
                {
                    fileStream.Position = 0;
                }

                var objectKey = string.IsNullOrWhiteSpace(normalizedFolder)
                    ? fileName
                    : $"{normalizedFolder}/{fileName}";

                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectKey,
                    InputStream = fileStream,
                    ContentType = string.IsNullOrWhiteSpace(contentType)
                        ? "application/octet-stream"
                        : contentType
                };

                await _amazonS3.PutObjectAsync(request, cancellationToken);
                return S3UploadResult.SuccessResponse(objectKey);
            }
            catch (AmazonS3Exception ex)
            {
                return S3UploadResult.FailureResponse(
                    "S3_UPLOAD_FAILED",
                    $"S3 upload failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return S3UploadResult.FailureResponse(
                    "S3_UPLOAD_ERROR",
                    $"Unexpected S3 upload error: {ex.Message}");
            }
        }

        private static string NormalizeFolderPath(string folderPath)
            => string.IsNullOrWhiteSpace(folderPath)
                ? string.Empty
                : folderPath.Trim().Trim('/');
    }
}
