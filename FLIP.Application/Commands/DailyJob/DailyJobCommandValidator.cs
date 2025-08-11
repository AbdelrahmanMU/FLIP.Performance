using FluentValidation;

namespace FLIP.Application.Commands.DailyJob;

public class DailyJobCommandValidator : AbstractValidator<DailyJobCommand>
{
    public DailyJobCommandValidator()
    {
        RuleFor(x => x.Freelancers)
            .NotNull()
            .NotEmpty();
    }
}
