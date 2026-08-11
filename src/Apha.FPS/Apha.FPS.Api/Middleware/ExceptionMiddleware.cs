using Apha.Common.Contracts;
using Apha.FPS.Application.Validation;
using Microsoft.AspNetCore.Authentication;
using Npgsql;
using System.Text.Json;


namespace Apha.FPS.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();

            var apiResponse = new ApiResponse<object>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError>(), // Ensure the Errors list is  initialized
                Meta = new ApiMeta
                {
                    CorrelationId = correlationId,
                    TimestampUtc = DateTime.UtcNow
                }
            };

            var errorType = _configuration["ExceptionTypes:General"]
                            ?? "FPSAPPS.EXCEPTION.FPS.GENERAL";

            switch (ex)
            {
                case UnauthorizedAccessException:
                case AuthenticationFailureException:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "AUTH_403",
                        Message = "Access denied."
                    });
                    errorType = _configuration["ExceptionTypes:Authorization"];
                    break;

                case BusinessValidationErrorException validationEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    foreach (var err in validationEx.Errors)
                    {
                        apiResponse.Errors.Add(new ApiError
                        {
                            Code = err.Code,
                            Message = err.Message,
                            Details = err.Details
                        });
                    }                    
                    break;
                case ArgumentException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "ARGUMENT_INVALID",
                        Message = ex.Message
                    });
                    break;
                case KeyNotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "RESOURCE_NOT_FOUND",
                        Message = ex.Message
                    });
                    break;
                case InvalidOperationException:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "BUSINESS_RULE_VIOLATION",
                        Message = ex.Message
                    });
                    break;
                case PostgresException pgEx:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "DB_POSTGRES_ERROR",
                        Message = "A database error occurred.",
                        Details = pgEx.MessageText
                    });
                    errorType = _configuration["ExceptionTypes:Database"];
                    break;
                case NpgsqlException:
                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "DB_CONNECTION_ERROR",
                        Message = "Database connection failed or service unavailable."
                    });
                    errorType = _configuration["ExceptionTypes:Database"];
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "SERVER_500",
                        Message = "An unexpected error occurred."
                    });
                    break;
            }

            LogException(ex, errorType!, apiResponse.Errors[0].Code, correlationId);

            var json = JsonSerializer.Serialize(apiResponse);
            await context.Response.WriteAsync(json);
        }

        private void LogException(          
          Exception ex,
          string errorType,
          string errorCode,
          string correlationId)
        {
            _logger.LogError(
                ex,
                "[{ErrorType}] [{ErrorCode}] CorrelationId:{CorrelationId} Message:{Message}",
                errorType,
                errorCode,
                correlationId,                
                ex.Message);
        }
    }
}
