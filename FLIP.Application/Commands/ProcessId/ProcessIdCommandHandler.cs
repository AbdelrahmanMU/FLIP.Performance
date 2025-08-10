using FLIP.Application.Interfaces;
using FLIP.Application.Models;
using MediatR;

namespace FLIP.Application.Commands.ProcessId;

public class ProcessIdCommandHandler(INotifyMessages notifyMessages) : IRequestHandler<ProcessIdCommand, Response>
{
    private readonly INotifyMessages _notifyMessages = notifyMessages;

    public async Task<Response> Handle(ProcessIdCommand request, CancellationToken cancellationToken)
    {
        return await _notifyMessages.NotifyFLIPQeueuAsync(request.Id, request.IsUpdating);
    }
}
