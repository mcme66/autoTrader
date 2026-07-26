using FinanceAnalysis.Application.Configuration;
using FinanceAnalysis.Application.Features.Authentication;

using FluentValidation;

using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Api.Validation;

internal sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IOptions<AuthenticationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var minimumLength = options.Value.MinimumPasswordLength;

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(minimumLength)
            .WithMessage($"The password must be at least {minimumLength} characters long.")
            .MaximumLength(256);
    }
}

internal sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
    }
}

internal sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator(IOptions<AuthenticationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var minimumLength = options.Value.MinimumPasswordLength;

        RuleFor(x => x.CurrentPassword).NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(minimumLength)
            .WithMessage($"The password must be at least {minimumLength} characters long.")
            .MaximumLength(256)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("The new password must differ from the current one.");
    }
}
