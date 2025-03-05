using FLIP.Performance.Config;
using FLIP.Performance.Helpers;
using FLIP.Performance.Models;
using Polly;
using Serilog;
using System.Diagnostics;

namespace FLIP.Performance.Utilities;

public class ApiCaller
{
    private readonly HttpClient _httpClient = new();
    private readonly int _retryCount = 5;
    private readonly int _maxDegreeOfParallelism = 16;
    private readonly Serilog.ILogger _logger = Log.Logger;
    private readonly List<ApiLog> _logs = [];
    private readonly List<FreelancerData> _freelancersData = [];
   
    public async Task<bool> CallExternalApiAsync(ApiRequest api, int id)
    {
        var policy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        HttpResponseMessage response = new();
        var log = new ApiLog();
        var freelancerData = new FreelancerData();
        var stopwatch = Stopwatch.StartNew(); // Track API response time

        try
        {
            response = await policy.ExecuteAsync(() =>
                _httpClient.GetAsync($"{api.Url}"));

            stopwatch.Stop();
            double responseTimeMs = stopwatch.Elapsed.TotalMilliseconds;

            log = new ApiLog
            {
                RequestUri = api.Url,
                StatusCode = (int)response.StatusCode,
                LoggedAt = DateTime.Now,
                Message = response.ReasonPhrase,
                ResponseTimeMs = responseTimeMs
            };

            if (response.IsSuccessStatusCode)
            {
                freelancerData = new FreelancerData
                {
                    PlatformName = api.PlatformName,
                    NationalId = id.ToString(),
                    CreatedDate = DateTime.Now,
                    JsonConvert = await response.Content.ReadAsStringAsync()
                };
            }

            _logs.Add(log);
            _freelancersData.Add(freelancerData);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error($"[API Caller] API call failed after {_retryCount} retries", ex.Message);
            log.Message = ex.Message;

            _logs.Add(log);

            return false;
        }
    }

    public async Task<(bool, List<ApiLog>, List<FreelancerData>)> ExecuteParallelApiCallsAsync(int id)
    {
        var tasks = new List<Task<bool>>();

        var apis = APIHelper.APIRequests();

        using (var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism))
        {
            var totalStopwatch = Stopwatch.StartNew(); // Start benchmark

            foreach (var api in apis)
            {
                api.Params.Insert(0, id.ToString());

                var apiWithParams = APIHelper.PrepareParams(api);

                await semaphore.WaitAsync(); // Control concurrency level

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        return await CallExternalApiAsync(apiWithParams, id);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            totalStopwatch.Stop();

            double totalExecutionTimeMs = totalStopwatch.Elapsed.TotalMilliseconds;

            _logger.Information($"Total response time is: {totalExecutionTimeMs}");

            await Task.WhenAll(tasks);
        }

        int successCount = tasks.Count(t => t.Result);
        _logger.Information($"Completed {tasks.Count} API calls, {successCount} were successful.");

        return (successCount >= 1, _logs, _freelancersData);
    }
}
