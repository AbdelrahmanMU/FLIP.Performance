using FLIP.Application.Models;
using MediatR;

namespace FLIP.Application.Queries.GetFreelancerData;

public class GetFreelancerQueryHandler : IRequestHandler<GetFreelancerQuery, ResponseVM<List<GetFreelancerDto>>>
{
    public Task<ResponseVM<List<GetFreelancerDto>>> Handle(GetFreelancerQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
