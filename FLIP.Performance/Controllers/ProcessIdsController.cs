using FLIP.Application.Commands.ProcessId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace FLIP.API.Controllers;

[Route("api/process-id/")]
public class ProcessIdsController(ISender sender) : ControllerBase
{
    public ISender _mediator { get; protected set; } = sender;

    [HttpPost("{id}")]
    public async Task<IActionResult> Process(string id)
    {
        await _mediator.Send(new ProcessIdCommand { Id = id });
        return Ok("Processing complete.");
    }

}
