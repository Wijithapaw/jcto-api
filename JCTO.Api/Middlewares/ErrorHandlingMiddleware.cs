using JCTO.Domain.CustomExceptions;
using JCTO.Domain.Dtos;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
                else if (ex is DbUpdateException)
                {
                    var dbEx = (DbUpdateException)ex;
                    if (dbEx.InnerException != null && dbEx.InnerException is PostgresException)
                    {
                        var pgEx = (PostgresException)ex.InnerException!;
                        if (!string.IsNullOrEmpty(pgEx.ConstraintName))
                        {
                            logger.LogWarning(ex, "Bad data. Database level constraint faild. {constraint} {column} {errorMessage}", pgEx.ConstraintName, pgEx.ColumnName, pgEx.MessageText);

                            statusCode = HttpStatusCode.BadRequest;
                            errorObj.ErrorMessage = $"Duplicate {pgEx.ConstraintName.Split('_').Last()}";
                        }
                    }
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
