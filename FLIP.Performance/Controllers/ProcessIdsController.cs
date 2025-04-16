using FLIP.Application.Commands.ProcessId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FLIP.API.Controllers;

[Authorize]
[Route("api/process-id/")]
public class ProcessIdsController(ISender sender) : ControllerBase
{
    public ISender _mediator { get; protected set; } = sender;

    [HttpPost("{id}")]
    public async Task<IActionResult> Process(string id)
    {
        return Ok(await _mediator.Send(new ProcessIdCommand { Id = id }));
    }

}
