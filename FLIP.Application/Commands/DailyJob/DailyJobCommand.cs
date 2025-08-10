using FLIP.Application.Models;
using MediatR;

namespace FLIP.Application.Commands.DailyJob;

public class DailyJobCommand : IRequest<Response>
{
    public string FreelancerId { get; set; } = default!;
}
