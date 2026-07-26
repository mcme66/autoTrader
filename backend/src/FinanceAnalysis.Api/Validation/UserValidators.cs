using FinanceAnalysis.Application.Features.Users;

using FluentValidation;

namespace FinanceAnalysis.Api.Validation;

internal sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator() =>
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(128);
}
