using FLIP.Application.Commands.ProcessId;
using FLIP.Application.Models;
using FLIP.Application.Queries.GetFreelancerData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FLIP.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ProcessIdsController(ISender sender) : ControllerBase
{
    public ISender Mediator { get; protected set; } = sender;

    [HttpPost("process-id/{id}")]
    public async Task<IActionResult> Process(string id)
    {
        return Ok(await Mediator.Send(new ProcessIdCommand { Id = id }));
    }

    [HttpGet("freelancer-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseVM<List<GetFreelancerDto>>>> Get([FromQuery] GetFreelancerQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }
}
