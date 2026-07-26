using System.Diagnostics;

using FinanceAnalysis.Domain.Exceptions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAnalysis.Api.Diagnostics;

/// <summary>
/// Translates unhandled exceptions into RFC 9457 problem responses.
/// </summary>
/// <remarks>
/// Domain exceptions carry a message written for the caller and map to a specific status code.
/// Everything else is a bug or an outage: the detail is replaced with a generic message and the
/// real one goes to the log alongside a trace identifier, so support can correlate a user's
/// report to a log entry without the response leaking internals.
/// </remarks>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var (status, title, detail) = Map(exception);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception on {Method} {Path}. Trace {TraceId}.",
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId);
        }
        else
        {
            logger.LogInformation(
                "Request to {Method} {Path} rejected with {StatusCode}: {Reason}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                status,
                exception.Message);
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Extensions = { ["traceId"] = traceId },
            },
        }).ConfigureAwait(false);
    }

    private static (int Status, string Title, string Detail) Map(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Resource not found", exception.Message),
        ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
        ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", exception.Message),
        UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
        BusinessRuleException => (StatusCodes.Status400BadRequest, "Request rejected", exception.Message),
        OperationCanceledException => (
            ProblemStatusCodes.ClientClosedRequest,
            "Request cancelled",
            "The request was cancelled before it completed."),
        _ => (
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred",
            "The request could not be completed. Quote the trace identifier when reporting this."),
    };
}

/// <summary>Status codes ASP.NET Core does not define.</summary>
internal static class ProblemStatusCodes
{
    /// <summary>nginx's code for a client that disconnected before the response was sent.</summary>
    public const int ClientClosedRequest = 499;
}
