using FLIP.Application.Commands.DailyJob;
using FLIP.Application.Interfaces;
using Hangfire;
using MediatR;
using Serilog;

namespace FLIP.API.BackgroundJobs;

public class RecallingApis(IRecurringJobManager recurringJobManager,
    ISender sender,
    IDapperQueries dapperQueries)
{
    private readonly Serilog.ILogger _logger = Log.Logger;
    private readonly IRecurringJobManager _recurringJobManager = recurringJobManager;
    private readonly ISender _mediator = sender;
    private readonly IDapperQueries _dapperQueries = dapperQueries;

    public void SchedulingTheJob()
    {
        _recurringJobManager.AddOrUpdate<RecallingApis>("recall-apis-job", job => job.CallApis(), Cron.Daily(0, 0));
    }

    public async Task CallApis()
    {
        _logger.Information("[Hangfire Job] The job started successfully");

        var prjectsUpdateInfo = await _dapperQueries.GetFreelancersProjectsUpdateInfo();
        var ridesUpdateInfo = await _dapperQueries.GetFreelancersRidesUpdateInfo();

        var freelancerData = prjectsUpdateInfo.Concat(ridesUpdateInfo);

        await _mediator.Send(new DailyJobCommand { Freelancers = [.. freelancerData] });
    }
}
