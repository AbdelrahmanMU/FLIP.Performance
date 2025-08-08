using BuildingBlock.Contracts;
using FLIP.Application.Helpers;
using FLIP.Application.Interfaces;
using MassTransit;

namespace FLIP.Infrastructure.Consumers;

public class FLIPRealTimeConsumer(IAPIIntegeration iAPIIntegeration) : IConsumer<PlatformRequestMessage>
{
    private readonly IAPIIntegeration _iAPIIntegeration = iAPIIntegeration;

    public async Task Consume(ConsumeContext<PlatformRequestMessage> context)
    {
        var platformRequest = context.Message;

        var apis = APIHelper.APIRequests();

        var freelancerDate = new Application.Models.FreelancerDto
        {
            Id = platformRequest.FreelancerId,
            Api = apis.FirstOrDefault(api => api.PlatformName == platformRequest.PlatformName) ?? new Application.Config.ApiRequest(),
            IsUpdating = false
        };

        await _iAPIIntegeration.ProcessId(freelancerDate);
    }
}
