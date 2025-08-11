using FLIP.Application.Models;
using MediatR;

namespace FLIP.Application.Queries.GetFreelancerData;

public class GetFreelancerQuery : IRequest<ResponseVM<List<GetFreelancerDto>>>
{
    public string FreelancerId { get; set; } = default!;
}
