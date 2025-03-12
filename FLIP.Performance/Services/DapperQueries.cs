using Dapper;
using FLIP.Performance.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FLIP.Performance.Services;

public class DapperQueries(IConfiguration configuration) : IDapperQueries
{
    private readonly string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";

    public async Task<int> InsertFreeelancers(List<FreelancerData> freelancersData)
    {
        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
                INSERT INTO StagingTableProject (TransactionID, PlatformName, IngestedAt, NationalId, JsonContent) 
                VALUES (@TransactionID, @PlatformName, @IngestedAt, @NationalId, @JsonContent);";

        var affectedRows = 0;

        affectedRows = await db.ExecuteAsync(sql, freelancersData);

        return affectedRows;
    }

    public async Task<int> InsertFreeelancersRide(List<FreelancerData> freelancersData)
    {
        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
                INSERT INTO StagingTableRide (TransactionID, PlatformName, IngestedAt, NationalId, JsonContent) 
                VALUES (@TransactionID, @PlatformName, @IngestedAt, @NationalId, @JsonContent);";

        var affectedRows = await db.ExecuteAsync(sql, freelancersData);

        return affectedRows;
    }

    public async Task<int> InsertLogs(List<ApiLog> apiLogs)
    {
        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
                INSERT INTO ApiLog (RequestUri, StatusCode, Message, LoggedAt, ResponseTimeMs) 
                VALUES (@RequestUri, @StatusCode, @Message, @LoggedAt, @ResponseTimeMs);";

        var affectedRows = await db.ExecuteAsync(sql, apiLogs);

        return affectedRows;
    }

    public async Task<int> InsertErrorLogs(List<ErrorLogs> apiLogs)
    {
        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
                INSERT INTO ErrorLogs (RequestUrl, RequestPayload, ErrorMessage, LoggedAt) 
                VALUES (@RequestUrl, @RequestPayload, @ErrorMessage, @LoggedAt);";

        var affectedRows = await db.ExecuteAsync(sql, apiLogs);

        return affectedRows;
    }
}
