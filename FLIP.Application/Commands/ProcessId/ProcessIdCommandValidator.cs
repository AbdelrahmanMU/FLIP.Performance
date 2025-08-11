using FluentValidation;

namespace FLIP.Application.Commands.ProcessId;

public class ProcessIdCommandValidator : AbstractValidator<ProcessIdCommand>
{
    public ProcessIdCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .NotEmpty();
    }
}
