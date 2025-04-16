using FLIP.Application.Interfaces;
using FLIP.Application.Models;
using MediatR;

namespace FLIP.Application.Commands.ProcessId;

public class ProcessIdCommandHandler(IAPIIntegeration aPIIntegeration,
    INotifyMessages notifyMessages) : IRequestHandler<ProcessIdCommand, Response>
{
    private readonly IAPIIntegeration _aPIIntegeration = aPIIntegeration;
    private readonly INotifyMessages _notifyMessages = notifyMessages;

    public async Task<Response> Handle(ProcessIdCommand request, CancellationToken cancellationToken)
    {
        var processIdResponse = await _aPIIntegeration.ProcessId(request);

        await _notifyMessages.NotifyBREAsync(int.Parse(request.Id));

        return processIdResponse;
    }
}
