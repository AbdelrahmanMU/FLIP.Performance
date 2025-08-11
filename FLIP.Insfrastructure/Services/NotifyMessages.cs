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

    public async Task<ResponseVM<List<PlatformMeta>>> NotifyFLIPRealTimeQeueuAsync(string freelancerId)
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

            var response = new ResponseVM<List<PlatformMeta>>
            {
                Status = "success",
                Error = null,
                Data = platformsMeta
            };

            return response;

        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);

            throw new Exception(ex.Message);
        }
    }

    public async Task<Application.Models.Response> NotifyDailyJobQeueuAsync(List<FreelancerDailyJobDto> freelancersDto)
    {
        var apis = APIHelper.APIRequests();
        var platformsMeta = new List<PlatformMeta>();

        try
        {
            var dailyJobMessage = new DailyJobMessage();

            foreach (var api in apis)
            {
                foreach (var freelancer in freelancersDto)
                {
                    dailyJobMessage.FreelancerId = freelancer.NationalId;
                    dailyJobMessage.PlatformName = api.PlatformName;
                    dailyJobMessage.TransactionID = freelancer.TransactionID;

                    await _publish.Publish(dailyJobMessage);

                    platformsMeta.Add(new PlatformMeta { PlatformName = api.PlatformName, IsSuccessfullyPublished = true });

                    _logger.Information($"[Publisher] Sent: {freelancer.NationalId}");
                }

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
