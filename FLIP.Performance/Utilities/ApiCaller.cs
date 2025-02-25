using FLIP.Performance.Config;
using FLIP.Performance.Helpers;
using Polly;
using Serilog;

namespace FLIP.Performance.Utilities;

public class ApiCaller
{
    private readonly HttpClient _httpClient = new();
    private readonly int _retryCount = 5;
    private readonly int _maxDegreeOfParallelism = 16;
    private readonly Serilog.ILogger _logger = Log.Logger;

    public async Task<bool> CallExternalApiAsync(ApiRequest api)
    {
        var policy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        try
        {
            var response = await policy.ExecuteAsync(() =>
                _httpClient.GetAsync($"{api.Url}"));

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.Error($"[API Caller] API call failed after {_retryCount} retries", ex.Message);
            return false;
        }
    }

    public async Task<bool> ExecuteParallelApiCallsAsync(int id)
    {
        var tasks = new List<Task<bool>>();

        var apis = APIHelper.APIRequests();
        
        using (var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism))
        {
            foreach (var api in apis)
            {
                api.Params.Insert(0, id.ToString());  

                var apiWithParams = APIHelper.PrepareParams(api);

                await semaphore.WaitAsync(); // Control concurrency level

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        return await CallExternalApiAsync(apiWithParams);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

        int successCount = tasks.Count(t => t.Result);
        _logger.Information($"Completed {tasks.Count} API calls, {successCount} were successful.");

        return tasks.Count == apis.Count;
    }

    


}
