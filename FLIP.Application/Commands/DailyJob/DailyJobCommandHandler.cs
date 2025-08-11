using FLIP.Application.Interfaces;
using MediatR;

namespace FLIP.Application.Commands.DailyJob;

public class DailyJobCommandHandler(INotifyMessages notifyMessages) : IRequestHandler<DailyJobCommand, Unit>
{
    private readonly INotifyMessages _notifyMessages = notifyMessages;

    public async Task<Unit> Handle(DailyJobCommand request, CancellationToken cancellationToken)
    {
        await _notifyMessages.NotifyDailyJobQeueuAsync(request.Freelancers);

        return Unit.Value;
    }
}
