using BuildingBlock.Contracts;
using FLIP.Application.Helpers;
using FLIP.Application.Interfaces;
using MassTransit;

namespace FLIP.Infrastructure.Consumers;

public class FLIPRealTimeConsumer(IAPIIntegeration iAPIIntegeration) : IConsumer<PlatformRequestMessage>
{
    private readonly IAPIIntegeration _iAPIIntegeration = iAPIIntegeration;

    public Task Consume(ConsumeContext<PlatformRequestMessage> context)
    {
		try
		{
            var platformRequest = context.Message;

            var apis = APIHelper.APIRequests();

            var freelancerDate = new Application.Models.FreelancerDto
            {
                Id = platformRequest.FreelancerId,
                Api = apis.FirstOrDefault(api => api.PlatformName == platformRequest.PlatformName) ?? new Application.Config.ApiRequest(),
                IsUpdating = false
            };

            _iAPIIntegeration.ProcessId(freelancerDate);

            return Task.CompletedTask;
        }
        catch (Exception)
		{
			throw;
		}

    }
}
