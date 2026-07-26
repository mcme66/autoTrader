using FinanceAnalysis.Application.Features.Portfolios;

using FluentValidation;

namespace FinanceAnalysis.Api.Validation;

internal sealed class CreatePortfolioRequestValidator : AbstractValidator<CreatePortfolioRequest>
{
    public CreatePortfolioRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(1000);

        RuleFor(x => x.BaseCurrency)
            .Length(3)
            .Matches("^[A-Za-z]{3}$")
            .WithMessage("The base currency must be a three-letter ISO 4217 code.")
            .When(x => !string.IsNullOrWhiteSpace(x.BaseCurrency));
    }
}

internal sealed class UpdatePortfolioRequestValidator : AbstractValidator<UpdatePortfolioRequest>
{
    public UpdatePortfolioRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

internal sealed class CreateHoldingRequestValidator : AbstractValidator<CreateHoldingRequest>
{
    public CreateHoldingRequestValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(16);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.AverageCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(1000);

        RuleFor(x => x.OpenedOn)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.OpenedOn.HasValue)
            .WithMessage("A position cannot be opened in the future.");
    }
}

internal sealed class UpdateHoldingRequestValidator : AbstractValidator<UpdateHoldingRequest>
{
    public UpdateHoldingRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.AverageCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(1000);

        RuleFor(x => x.OpenedOn)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.OpenedOn.HasValue)
            .WithMessage("A position cannot be opened in the future.");
    }
}
