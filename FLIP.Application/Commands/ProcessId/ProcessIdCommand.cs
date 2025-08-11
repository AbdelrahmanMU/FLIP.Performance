using FLIP.Application.Models;
using MediatR;

namespace FLIP.Application.Commands.ProcessId;

public class ProcessIdCommand : IRequest<ResponseVM<List<PlatformMeta>>>
{
    public string Id { get; set; } = default!;
}
