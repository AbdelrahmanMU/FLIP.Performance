using BuildingBlock.Contracts;
using FLIP.Application.Helpers;
using FLIP.Application.Interfaces;
using MassTransit;
using Serilog;

namespace FLIP.Infrastructure.Consumers;

public class DailyJobConsumer(IAPIIntegeration iAPIIntegeration,
    IDapperQueries dapperQueries) : IConsumer<DailyJobMessage>
{
    private readonly IAPIIntegeration _iAPIIntegeration = iAPIIntegeration;
    private readonly IDapperQueries _dapperQueries = dapperQueries;
    private readonly ILogger _logger = Log.Logger;

    public async Task Consume(ConsumeContext<DailyJobMessage> context)
    {
        var platformRequest = context.Message;

        var apis = APIHelper.APIRequests();

        var freelancerDate = new Application.Models.FreelancerDto
        {
            Id = platformRequest.FreelancerId,
            TransactionID = platformRequest.TransactionID,
            Api = apis.FirstOrDefault(api => api.PlatformName == platformRequest.PlatformName) ?? new Application.Config.ApiRequest(),
            IsUpdating = true
        };

        var partenerResponse = await _iAPIIntegeration.ProcessId(freelancerDate);

        if (partenerResponse.Success)
        {
            try
            {
                var rides = partenerResponse is not null
                    && partenerResponse.FreelancerData is not null
                    && partenerResponse.FreelancerData.IsRide
                    ? partenerResponse.FreelancerData
                    : null;

                var projects = partenerResponse is not null
                    && partenerResponse.FreelancerData is not null &&
                    !partenerResponse.FreelancerData.IsRide
                    ? partenerResponse.FreelancerData
                    : null;

                if (rides is not null)
                {
                    await _dapperQueries.UpdateFreelancersRide(rides);
                }

                if (projects is not null)
                {
                    await _dapperQueries.UpdateFreelancersProjects(projects);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"[Insert in DB] Error while inserting in db: {ex.Message}", ex.Message);
            }
        }
        else
        {
            _logger.Warning("There is no successeded api request");
        }
    }
}
