using FLIP.Application.Interfaces;
using FLIP.Application.Models;
using MediatR;

namespace FLIP.Application.Commands.DailyJob;

public class DailyJobCommandHandler(INotifyMessages notifyMessages) : IRequestHandler<DailyJobCommand, Response>
{
    private readonly INotifyMessages _notifyMessages = notifyMessages;

    public async Task<Response> Handle(DailyJobCommand request, CancellationToken cancellationToken)
    {
        return await _notifyMessages.NotifyDailyJobQeueuAsync(request.FreelancerId);
    }
}
