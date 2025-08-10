using BuildingBlock.Contracts;
using FLIP.Application.Helpers;
using FLIP.Application.Interfaces;
using FLIP.Application.Models;
using MassTransit;
using Serilog;
using System.Net;

namespace FLIP.Infrastructure.Services;

public class NotifyMessages(IPublishEndpoint publish) : INotifyMessages
{
    private readonly ILogger _logger = Log.Logger;
    private readonly IPublishEndpoint _publish = publish;

    public async Task<Application.Models.Response> NotifyFLIPRealTimeQeueuAsync(string freelancerId)
    {
        var apis = APIHelper.APIRequests();
        var platformsMeta = new List<PlatformMeta>();

        try
        {
            var realTimeMessage = new PlatformRequestMessage();

            foreach (var api in apis)
            {
                realTimeMessage.FreelancerId = freelancerId;
                realTimeMessage.PlatformName = api.PlatformName;

                await _publish.Publish(realTimeMessage);

                platformsMeta.Add(new PlatformMeta { PlatformName = api.PlatformName, IsSuccessfullyPublished = true });

                _logger.Information($"[Publisher] Sent: {freelancerId}");
            }

            var response = new Application.Models.Response
            {
                Success = true,
                StatusCode = (int)HttpStatusCode.OK,
                PlatformsMeta = platformsMeta
            };

            return response;

        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);

            throw new Exception(ex.Message);
        }
    }

    public async Task<Application.Models.Response> NotifyDailyJobQeueuAsync(string freelancerId)
    {
        var apis = APIHelper.APIRequests();
        var platformsMeta = new List<PlatformMeta>();

        try
        {
            var dailyJobMessage = new DailyJobMessage();

            foreach (var api in apis)
            {
                dailyJobMessage.FreelancerId = freelancerId;
                dailyJobMessage.PlatformName = api.PlatformName;

                await _publish.Publish(dailyJobMessage);

                platformsMeta.Add(new PlatformMeta { PlatformName = api.PlatformName, IsSuccessfullyPublished = true });

                _logger.Information($"[Publisher] Sent: {freelancerId}");
            }

            var response = new Application.Models.Response
            {
                Success = true,
                StatusCode = (int)HttpStatusCode.OK,
                PlatformsMeta = platformsMeta
            };

            return response;

        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);

            throw new Exception(ex.Message);
        }
    }
}
