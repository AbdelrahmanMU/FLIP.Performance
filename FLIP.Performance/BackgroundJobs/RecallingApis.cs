using FLIP.Application.Commands.DailyJob;
using FLIP.Application.Commands.ProcessId;
using FLIP.Application.Interfaces;
using Hangfire;
using MediatR;
using Serilog;

namespace FLIP.API.BackgroundJobs;

public class RecallingApis(IDapperQueries dapperQueries,
    IRecurringJobManager recurringJobManager,
    ISender sender)
{
    private readonly IDapperQueries _dapperQueries = dapperQueries;
    private readonly Serilog.ILogger _logger = Log.Logger;
    private readonly IRecurringJobManager _recurringJobManager = recurringJobManager;
    private readonly ISender _mediator = sender;

    public void SchedulingTheJob()
    {
        _recurringJobManager.AddOrUpdate<RecallingApis>("recall-apis-job", job => job.CallApis(), Cron.Daily(0, 0));
    }

    public async Task CallApis()
    {
        _logger.Information("[Hangfire Job] The job started successfully");

        var ids = await _dapperQueries.GetFreelancersIds();

        var distinctIds = ids.Distinct();

        foreach (var id in distinctIds)
        {
            var processIdResult = await _mediator.Send(new DailyJobCommand { FreelancerId = id});

            await _dapperQueries.InsertLogs(processIdResult.ApiLogData);
            await _dapperQueries.InsertErrorLogs(processIdResult.ErrorLogsData);

            if (processIdResult.Success)
            {
                try
                {
                    var rides = processIdResult is not null 
                        && processIdResult.FreelancerData is not null 
                        && processIdResult.FreelancerData.IsRide 
                        ? processIdResult.FreelancerData 
                        : null ;

                    var projects = processIdResult is not null 
                        && processIdResult.FreelancerData is not null &&
                        processIdResult.FreelancerData.IsRide 
                        ? processIdResult.FreelancerData 
                        : null;

                    if (rides is not null)
                    {
                        await _dapperQueries.UpdateFreelancersRide(rides);
                    }

                    if (projects is not null)
                    {
                        await _dapperQueries.UpdateFreelancers(projects);
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

}
