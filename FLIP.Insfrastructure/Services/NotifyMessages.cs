using BuildingBlock.Contracts;
using FLIP.Application.Helpers;
using FLIP.Application.Interfaces;
using MassTransit;
using Serilog;

namespace FLIP.Infrastructure.Services;

public class NotifyMessages(IPublishEndpoint publish) : INotifyMessages
{
    private readonly ILogger _logger = Log.Logger;
    private readonly IPublishEndpoint _publish = publish;

    public async Task NotifyFLIPRealTimeAsync(string freelancerId)
    {
        var apis = APIHelper.APIRequests();

        try
        {
            var message = new PlatformRequestMessage();

            foreach (var api in apis)
            {
                message.FreelancerId = freelancerId;
                message.PlatformName = api.PlatformName;

                await _publish.Publish(message);

                _logger.Information($"[Publisher] Sent: {freelancerId}");
            }
            
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);

            throw new Exception(ex.Message);
        }
    }

    public Task NotifyDailyJobAsync(int number, Application.Models.Response apiResponse)
    {
        throw new NotImplementedException();
    }
}
