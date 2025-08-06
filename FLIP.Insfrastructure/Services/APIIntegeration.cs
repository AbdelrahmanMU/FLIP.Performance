using FLIP.Application.Config;
using FLIP.Application.Helpers;
using FLIP.Application.Interfaces;
using FLIP.Application.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Timeout;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace FLIP.Infrastructure.Services;

public class APIIntegeration(IConfiguration configuration,
    IDapperQueries dapperQueries,
    IMemoryCache memoryCache) : IAPIIntegeration
{
    private readonly HttpClient _httpClient = new();
    private readonly int _maxDegreeOfParallelism = 16;
    private readonly ILogger _logger = Log.Logger;
    private ApiLog _log = new();
    private ErrorLogs _errorLog = new();
    private FreelancerData _freelancersData = new();
    private readonly IConfiguration _configuration = configuration;
    private readonly IDapperQueries _dapperQueries = dapperQueries;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<Response> ProcessId(FreelancerDto freelancer)
    {
        var stopwatch = Stopwatch.StartNew();

        //if (_memoryCache.TryGetValue(freelancer.Id, out _))
        //{
        //    // ID is already cached
        //    return new Response
        //    {
        //        Success = true,
        //        StatusCode = (int)HttpStatusCode.OK,
        //    };
        //}

        //// ID is not cached, so we add it
        //_memoryCache.Set(freelancer.Id, true);

        // At least one success
        var (success, apiLogs, freelancerDataResponse, errorLogs) = await ExecuteParallelApiCallsAsync(freelancer);
        if (success)
        {
            try
            {
                if (freelancer.IsUpdating)
                {
                    return new Response
                    {
                        Success = true,
                        StatusCode = (int)HttpStatusCode.OK,
                        FreelancerData = freelancerDataResponse,
                        ApiLogData = apiLogs,
                        ErrorLogsData = errorLogs
                    };
                }

                var rides = freelancerDataResponse.IsRide ? freelancerDataResponse : new FreelancerData();

                var projects = !freelancerDataResponse.IsRide ? freelancerDataResponse : new FreelancerData();

                if (rides is not null)
                {
                    await _dapperQueries.InsertFreelancerRide(rides);
                }

                if (projects is not null)
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

            throw new Exception("There is no any API call succeed");
        }
    }

    protected virtual async Task<(bool, ApiLog, FreelancerData, ErrorLogs)> ExecuteParallelApiCallsAsync(FreelancerDto freelancerDto)
    {
        var tasks = new List<Task<bool>>();

        var apis = APIHelper.APIRequests();

        using (var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism))
        {
            var totalStopwatch = Stopwatch.StartNew(); // Start benchmark

            freelancerDto.Api.Params.Insert(0, freelancerDto.Id);

            freelancerDto.Api.PrepareParams();

            await semaphore.WaitAsync(); // Control concurrency level

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    return await CallExternalApiAsync(freelancerDto.Api, freelancerDto.Id);
                }
                finally
                {
                    semaphore.Release();
                }
            }));

            totalStopwatch.Stop();

            double totalExecutionTimeMs = totalStopwatch.Elapsed.TotalMilliseconds;

            _logger.Information($"Total response time is: {totalExecutionTimeMs}");

            await Task.WhenAll(tasks);
        }

        int successCount = tasks.Count(t => t.Result);
        _logger.Information($"Completed {tasks.Count} API calls, {successCount} were successful.");

        return (successCount >= 1, _log, _freelancersData, _errorLog);
    }

    private async Task<bool> CallExternalApiAsync(ApiRequest api, string id)
    {
        var retryCount = int.Parse(_configuration["retryCount"] ?? "");
        var maxResponseTime = int.Parse(_configuration["maxResponseTime"] ?? "");

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(maxResponseTime));
        var cancellationToken = cts.Token;

        var policy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != HttpStatusCode.NotFound)
            .Or<TimeoutRejectedException>()
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

            response = await policy.ExecuteAsync(async (ct) =>
            {
                // Perform the HTTP GET request with the cancellation token
                return await _httpClient.GetAsync($"{api.Url}", ct);
            }, cancellationToken);

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

                _freelancersData = freelancerData;
            }

            _log = log;

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error($"[API Caller] API call failed after {retryCount} retries", ex.Message);
           
            log.Message = ex.Message;
            log.RequestUri = api.Url;
            log.StatusCode = (int)response.StatusCode;

            var errorLog = new ErrorLogs
            {
                RequestUrl = api.Url,
                RequestPayload = id.ToString(),
                ErrorMessage = ex.Message,
                LoggedAt = DateTime.UtcNow
            };

            _log = log;
            _errorLog = errorLog;

            throw new Exception(ex.Message);
        }
    }
}
