namespace Apha.Common.Utilities.Storage
{
    public class S3UploadResult
    {
        public bool Success { get; set; }
        public string? ObjectKey { get; set; }
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }

        public static S3UploadResult SuccessResponse(string objectKey) => new()
        {
            Success = true,
            ObjectKey = objectKey
        };

        public static S3UploadResult FailureResponse(string errorCode, string message) => new()
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message
        };
    }
}
