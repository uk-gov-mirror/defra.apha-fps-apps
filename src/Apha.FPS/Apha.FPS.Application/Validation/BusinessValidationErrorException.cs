namespace Apha.FPS.Application.Validation
{
    public class BusinessValidationErrorException : Exception
    {
        public string Status { get; set; } = "error";
        public string ExceptionMessage { get; set; } = "Business validation failed.";

        public List<BusinessValidationError> Errors { get; set; }

        public BusinessValidationErrorException(List<BusinessValidationError> errors)
            : base(BuildMessage(errors))
        {
            Errors = errors;
        }

        private static string BuildMessage(List<BusinessValidationError> errors) =>
            errors is { Count: > 0 }
                ? string.Join("; ", errors.Select(e => e.Message))
                : "Business validation failed.";
    }
}
