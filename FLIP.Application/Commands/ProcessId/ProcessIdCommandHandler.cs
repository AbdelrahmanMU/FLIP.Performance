using FLIP.Application.Interfaces;
using FLIP.Application.Models;
using MediatR;

namespace FLIP.Application.Commands.ProcessId;

public class ProcessIdCommandHandler(INotifyMessages notifyMessages) : IRequestHandler<ProcessIdCommand, ResponseVM<List<PlatformMeta>>>
{
    private readonly INotifyMessages _notifyMessages = notifyMessages;

    public async Task<ResponseVM<List<PlatformMeta>>> Handle(ProcessIdCommand request, CancellationToken cancellationToken)
    {
        return await _notifyMessages.NotifyFLIPRealTimeQeueuAsync(request.Id);
    }
}
