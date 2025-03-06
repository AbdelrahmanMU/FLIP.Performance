using Dapper;
using FLIP.Performance.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FLIP.Performance.Services;

public class DapperQueries : IDapperQueries
{
    private readonly string connectionString = "Server=10.10.208.93; Database={DB_Name}; User ID=sa; Password=sa@123456; TrustServerCertificate=True; Connection Timeout=30;";

    public async Task<int> InsertFreeelancers(List<FreelancerData> freelancersData)
    {
        using IDbConnection db = new SqlConnection(connectionString);
        
        string sql = @"
                INSERT INTO FreelancerData (PlatformName, TransactionID, NationalId, IntegeratedAt, JsonConvert) 
                VALUES (@PlatformName, @TransactionID, @NationalId, @IntegeratedAt, @JsonConvert);";

        var affectedRows = await db.ExecuteAsync(sql, freelancersData);

        return affectedRows;
    }

    public async Task<int> InsertLogs(List<ApiLog> apiLogs)
    {
        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
                INSERT INTO ApiLog (RequestUri, StatusCode, Message, LoggedAt) 
                VALUES (@RequestUri, @StatusCode, @Message, @LoggedAt);";

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
