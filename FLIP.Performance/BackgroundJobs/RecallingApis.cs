using FLIP.Application.Commands.ProcessId;
using FLIP.Application.Interfaces;
using FLIP.Application.Models;
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
            var processIdResult = await _mediator.Send(new ProcessIdCommand { Id = id, IsUpdating = true });

            if (processIdResult.Success)
            {
                try
                {
                    var rides = processIdResult.FreelancerData
                    .Where(x => x.IsRide)
                    .DistinctBy(x => x.NationalId).ToList();

                    var projects = processIdResult.FreelancerData
                        .Where(x => !x.IsRide)
                        .DistinctBy(x => x.NationalId).ToList();

                    if (rides.Count != 0)
                    {
                        await _dapperQueries.UpdateFreelancersRide(rides);
                    }

                    if (projects.Count != 0)
                    {
                        await _dapperQueries.UpdateFreelancers(projects);
                    }

                    await _dapperQueries.InsertLogs(processIdResult.ApiLogData);
                    await _dapperQueries.InsertErrorLogs(processIdResult.ErrorLogsData);
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
