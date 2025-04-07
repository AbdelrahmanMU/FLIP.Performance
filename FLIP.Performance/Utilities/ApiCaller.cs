using FLIP.Performance.Config;
using FLIP.Performance.Helpers;
using FLIP.Performance.Models;
using Polly;
using Serilog;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace FLIP.Performance.Utilities;

public class ApiCaller(IConfiguration configuration)
{
    private readonly HttpClient _httpClient = new();
    private readonly int _maxDegreeOfParallelism = 16;
    private readonly Serilog.ILogger _logger = Log.Logger;
    private readonly List<ApiLog> _logs = [];
    private readonly List<ErrorLogs> _errorLogs = [];
    private readonly List<FreelancerData> _freelancersData = [];
    private readonly IConfiguration _configuration = configuration;

    public async Task<bool> CallExternalApiAsync(ApiRequest api, int id/*, FreelancerData  freelancer*/)
    {
        var retryCount = int.Parse(_configuration["retryCount"] ?? "");

        var policy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        HttpResponseMessage response = new();
        var log = new ApiLog();
        var freelancerData = new FreelancerData();
        var stopwatch = Stopwatch.StartNew(); // Track API response time

        try
        {
            if (api.IsBearerAuth)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", api.BearerToken);
            }

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
                    TransactionID = Guid.NewGuid(),
                    PlatformName = api.PlatformName,
                    IngestedAt = DateTime.Now.AddDays(-1),
                    NationalId = id.ToString(),
                    JsonContent = await response.Content.ReadAsStringAsync()
                    
                    
                    //TransactionID = freelancer.TransactionID,
                    //PlatformName = freelancer.PlatformName,
                    //IngestedAt = DateTime.Now.AddDays(-1),
                    //NationalId = freelancer.NationalId,
                    //JsonContent = freelancer.JsonContent
                };

                _freelancersData.Add(freelancerData);
            }

            _logs.Add(log);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error($"[API Caller] API call failed after {retryCount} retries", ex.Message);
            log.Message = ex.Message;

            var errorLog = new ErrorLogs
            {
                RequestUrl = api.Url,
                RequestPayload = id.ToString(),
                ErrorMessage = ex.Message,
                LoggedAt = DateTime.UtcNow
            };

            _logs.Add(log);

            return false;
        }
    }

    public async Task<(bool, List<ApiLog>, List<FreelancerData>, List<ErrorLogs>)> ExecuteParallelApiCallsAsync(int id)
    {
        var tasks = new List<Task<bool>>();

        var apis = APIHelper.APIRequests();
        //var dummyData = APIHelper.DumyData();

        using (var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism))
        {
            var totalStopwatch = Stopwatch.StartNew(); // Start benchmark
            int i = 0;

            foreach (var api in apis)
            {
                api.Params.Insert(0, id.ToString());

                api.PrepareParams();

                await semaphore.WaitAsync(); // Control concurrency level

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        return await CallExternalApiAsync(api, id);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));

                i++;
            }

            totalStopwatch.Stop();

            double totalExecutionTimeMs = totalStopwatch.Elapsed.TotalMilliseconds;

            _logger.Information($"Total response time is: {totalExecutionTimeMs}");

            await Task.WhenAll(tasks);
        }

        int successCount = tasks.Count(t => t.Result);
        _logger.Information($"Completed {tasks.Count} API calls, {successCount} were successful.");

        return (successCount >= 1, _logs, _freelancersData, _errorLogs);
    }
}
