using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using System.Net;

namespace JCTO.Api.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<ErrorHandlingMiddleware> logger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var statusCode = HttpStatusCode.InternalServerError;
                var errorObj = new ErrorResponse();

                if (ex is JCTOValidationException)
                {
                    logger.LogWarning("Validation error. {errorMessage}", ex.Message);

                    statusCode = HttpStatusCode.BadRequest;
                    errorObj.ErrorMessage = ex.Message;
                }
                else if (ex is JCTOConcurrencyException)
                {
                    var concurrencyEx = (JCTOConcurrencyException)ex;
                    logger.LogWarning(ex.Message);

                    statusCode = HttpStatusCode.Conflict;
                    errorObj.ErrorMessage = $"'{concurrencyEx.Entity}', you are updating is outdated. Please refresh the '{concurrencyEx.Entity}' and update again.";
                }
                else if (ex is JCTOException)
                {
                    logger.LogWarning(ex, "Handled error. {errorMessage}", ex.Message);

                    errorObj.ErrorMessage = ex.Message;
                }
                else
                {
                    logger.LogError(ex, "Unhandled error. {errorMessage}", ex.Message);

                    errorObj.ErrorMessage = "Unknown error";
                }

                context.Response.StatusCode = (int)statusCode;
                await context.Response.WriteAsJsonAsync(errorObj);
            }
        }
    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
