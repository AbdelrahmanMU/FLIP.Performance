using FLIP.Application.Models;
using MediatR;

namespace FLIP.Application.Commands.DailyJob;

public class DailyJobCommand : IRequest<Unit>
{
    public List<FreelancerDailyJobDto> Freelancers { get; set; } = [];
}

