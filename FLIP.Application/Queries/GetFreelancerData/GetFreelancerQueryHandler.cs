using FLIP.Application.Interfaces;
using FLIP.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FLIP.Application.Queries.GetFreelancerData;

public class GetFreelancerQueryHandler(IDapperQueries dapperQueries) : IRequestHandler<GetFreelancerQuery, ResponseVM<List<GetFreelancerDto>>>
{
    private readonly IDapperQueries _dapperQueries = dapperQueries;

    public async Task<ResponseVM<List<GetFreelancerDto>>> Handle(GetFreelancerQuery request, CancellationToken cancellationToken)
    {
		try
		{
            var freelancerData = await _dapperQueries.GetFreelancerData(request.FreelancerId);

            return new ResponseVM<List<GetFreelancerDto>>
            {
                Status = "success",
                Data = freelancerData,
            };
        }
		catch (Exception ex)
		{
            return new ResponseVM<List<GetFreelancerDto>>
            {
                Status = "error",
                Error = new ApiError
                {
                    ErrorCode = StatusCodes.Status500InternalServerError.ToString(),
                    ErrorDescription = ex.Message,

                }
            };
		}
    }
}
