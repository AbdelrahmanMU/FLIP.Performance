using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using FLIP.Application.Commands.ProcessId;
using FLIP.Application.Config;
using FLIP.Application.Helpers;
using FLIP.Application.Interfaces;
using FLIP.Application.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Polly;
using Serilog;

namespace FLIP.Infrastructure.Services;

public class APIIntegeration(IConfiguration configuration, 
    IDapperQueries dapperQueries,
    IMemoryCache memoryCache) : IAPIIntegeration
{
    private readonly HttpClient _httpClient = new();
    private readonly int _maxDegreeOfParallelism = 16;
    private readonly ILogger _logger = Log.Logger;
    private readonly List<ApiLog> _logs = [];
    private readonly List<ErrorLogs> _errorLogs = [];
    private readonly List<FreelancerData> _freelancersData = [];
    private readonly IConfiguration _configuration = configuration;
    private readonly IDapperQueries _dapperQueries = dapperQueries;
    private readonly IMemoryCache _memoryCache = memoryCache;
        
    public async Task<Response> ProcessId(ProcessIdCommand request)
    {
        var stopwatch = Stopwatch.StartNew();

        if (_memoryCache.TryGetValue(request.Id, out _))
        {
            // ID is already cached
            return new Response
            {
                Success = true,
                StatusCode = (int)HttpStatusCode.OK,
            };
        }

        // ID is not cached, so we add it
        _memoryCache.Set(request.Id, true);

        // At least one success
        var (success, apiLogs, freelancerDataResponse, errorLogs) = await ExecuteParallelApiCallsAsync(request.Id);
        if (success)
        {
            try
            {
                var rides = freelancerDataResponse.Where(x => x.IsRide).ToList();
                var projects = freelancerDataResponse.Where(x => !x.IsRide).ToList();

                if (rides.Count != 0)
                {
                    await _dapperQueries.InsertFreelancersRide(rides);
                }

                if (projects.Count != 0)
                {
                    await _dapperQueries.InsertFreelancers(projects);
                }

                await _dapperQueries.InsertLogs(apiLogs);
                await _dapperQueries.InsertErrorLogs(errorLogs);

                stopwatch.Stop();

                return new Response
                {
                    Success = true,
                    StatusCode = (int)HttpStatusCode.OK,
                    FreelancerData = freelancerDataResponse,
                    ApiLogData = apiLogs,
                    ErrorLogsData = errorLogs
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.Error("Failed to update the DB:", ex.Message);
                throw new Exception(ex.Message);
            }
        }
        else
        {
            _logger.Error("Failed to ExecuteParallelApiCallsAsync");

            stopwatch.Stop();

            return new Response
            {
                Success = false,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "There is no any API get successed",
                Errors = apiLogs.Select(x => x.Message).ToList()
            };
        }
    }

    protected virtual async Task<(bool, List<ApiLog>, List<FreelancerData>, List<ErrorLogs>)> ExecuteParallelApiCallsAsync(string id)
    {
        var tasks = new List<Task<bool>>();

        var apis = APIHelper.APIRequests();

        using (var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism))
        {
            var totalStopwatch = Stopwatch.StartNew(); // Start benchmark
            int i = 0;

            foreach (var api in apis)
            {
                api.Params.Insert(0, id);

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

    private async Task<bool> CallExternalApiAsync(ApiRequest api, string id)
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
                    NationalId = id,
                    JsonContent = await response.Content.ReadAsStringAsync(),
                    IsRide = api.IsRide
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

            throw new Exception(ex.Message);
        }
    }
}
