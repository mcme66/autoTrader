using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FinanceAnalysis.Api.Filters;

/// <summary>
/// Runs the FluentValidation validator for every bound action argument that has one.
/// </summary>
/// <remarks>
/// FluentValidation's own MVC auto-validation package is deprecated, so validation is invoked
/// explicitly here. Doing it in a filter rather than in each action means a new endpoint is
/// validated the moment someone writes a validator for its request type, with no chance of a
/// controller forgetting the call. Failures are returned as RFC 9457 validation problems.
/// </remarks>
internal sealed class ValidationActionFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var errors = new ModelStateDictionary();

        foreach (var (name, argument) in context.ActionArguments)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (services.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var result = await validator
                .ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted)
                .ConfigureAwait(false);

            foreach (var failure in result.Errors)
            {
                errors.AddModelError(
                    string.IsNullOrEmpty(failure.PropertyName) ? name : failure.PropertyName,
                    failure.ErrorMessage);
            }
        }

        if (!errors.IsValid)
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
            });

            return;
        }

        await next().ConfigureAwait(false);
    }
}
