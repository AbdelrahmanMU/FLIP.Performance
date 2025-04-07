using FLIP.Performance.Services;
using FLIP.Performance.Utilities;
using Hangfire;
using Serilog;

namespace FLIP.Performance.BackgroundJobs;

public class RecallingApis(IDapperQueries dapperQueries,
    IConfiguration configuration,
    IRecurringJobManager recurringJobManager)
{
    private readonly IDapperQueries _dapperQueries = dapperQueries;
    private readonly ApiCaller _apiCaller = new(configuration);
    private readonly Serilog.ILogger _logger = Log.Logger;
    private readonly IRecurringJobManager _recurringJobManager = recurringJobManager;

    public void SchedulingTheJob()
    {
        _recurringJobManager.AddOrUpdate<RecallingApis>("recall-apis-job", job => job.CallApis(), Cron.Daily(0, 0));
    }

    public async Task CallApis()
    {
        _logger.Information("[Hangfire Job] The job started successfully");

        var ids = await _dapperQueries.GetFreelancersIds();

        foreach (var id in ids) 
        {
            var (success, apiLogs, freelancerDataResponse, errorLogs) = await _apiCaller.ExecuteParallelApiCallsAsync(int.Parse(id));
           
            if (success)
            {
                try
                {
                    await _dapperQueries.InsertLogs(apiLogs);
                    await _dapperQueries.UpdateFreelancers(freelancerDataResponse);
                    await _dapperQueries.UpdateFreelancersRide(freelancerDataResponse);
                    await _dapperQueries.InsertErrorLogs(errorLogs);
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
