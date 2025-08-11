using Dapper;
using FLIP.Application.Interfaces;
using FLIP.Application.Models;
using FLIP.Application.Queries.GetFreelancerData;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FLIP.Infrastructure.Services;

public class DapperQueries(IConfiguration configuration) : IDapperQueries
{
    private readonly string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";

    #region Insert

    public async Task<int> InsertFreelancers(FreelancerData? freelancersData)
    {
        ArgumentNullException.ThrowIfNull(freelancersData);

        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
                INSERT INTO StagingTableProject (TransactionID, PlatformName, IngestedAt, NationalId, JsonContent) 
                VALUES (@TransactionID, @PlatformName, @IngestedAt, @NationalId, @JsonContent);";

        var affectedRows = 0;

        affectedRows = await db.ExecuteAsync(sql, freelancersData);

        return affectedRows;
    }

    public async Task<int> InsertFreelancerRide(FreelancerData? freelancersData)
    {
        ArgumentNullException.ThrowIfNull(freelancersData);

        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
                INSERT INTO StagingTableRide (TransactionID, PlatformName, IngestedAt, NationalId, JsonContent) 
                VALUES (@TransactionID, @PlatformName, @IngestedAt, @NationalId, @JsonContent);";

        var affectedRows = await db.ExecuteAsync(sql, freelancersData);

        return affectedRows;
    }

    public async Task<int> InsertLogs(ApiLog apiLogs)
    {
        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
                INSERT INTO ApiLog (RequestUri, StatusCode, Message, LoggedAt, ResponseTimeMs) 
                VALUES (@RequestUri, @StatusCode, @Message, @LoggedAt, @ResponseTimeMs);";

        var affectedRows = await db.ExecuteAsync(sql, apiLogs);

        return affectedRows;
    }

    public async Task<int> InsertErrorLogs(ErrorLogs? errorLogs)
    {
        ArgumentNullException.ThrowIfNull(errorLogs);

        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
                INSERT INTO ErrorLogs (RequestUrl, RequestPayload, ErrorMessage, LoggedAt) 
                VALUES (@RequestUrl, @RequestPayload, @ErrorMessage, @LoggedAt);";

        var affectedRows = await db.ExecuteAsync(sql, errorLogs);

        return affectedRows;
    }

    #endregion

    #region Update

    public async Task<int> UpdateFreelancersProjects(FreelancerData? freelancersData)
    {
        ArgumentNullException.ThrowIfNull(freelancersData);

        using IDbConnection db = new SqlConnection(connectionString);

        const string sql = @"
        UPDATE StagingTableProject
        SET 
            PlatformName = @PlatformName,
            IngestedAt   = @IngestedAt,
            JsonContent  = @JsonContent,
            NationalId   = @NationalId
        WHERE TransactionID = @TransactionID;";

        var affectedRows = await db.ExecuteAsync(sql, freelancersData);

        return affectedRows;
    }

    public async Task<int> UpdateFreelancersRide(FreelancerData? freelancersData)
    {
        ArgumentNullException.ThrowIfNull(freelancersData);

        using IDbConnection db = new SqlConnection(connectionString);

        const string sql = @"
        UPDATE StagingTableRide
        SET 
            PlatformName = @PlatformName,
            IngestedAt   = @IngestedAt,
            JsonContent  = @JsonContent,
            NationalId   = @NationalId
        WHERE TransactionID = @TransactionID;";

        var affectedRows = await db.ExecuteAsync(sql, freelancersData);

        return affectedRows;
    }

    #endregion

    #region Get

    public async Task<List<FreelancerDailyJobDto>> GetFreelancersProjectsUpdateInfo()
    {
        using IDbConnection db = new SqlConnection(connectionString);

        string selectSql = @"
        SELECT NationalId, TransactionID 
        FROM StagingTableProject";

        var result = await db.QueryAsync<FreelancerDailyJobDto>(selectSql);

        return [.. result];
    }

    public async Task<List<FreelancerDailyJobDto>> GetFreelancersRidesUpdateInfo()
    {
        using IDbConnection db = new SqlConnection(connectionString);

        string selectSql = @"
        SELECT NationalId, TransactionID
        FROM StagingTableRide";

        var result = await db.QueryAsync<FreelancerDailyJobDto>(selectSql);

        return [.. result];
    }

    public async Task<List<GetFreelancerDto>> GetFreelancerData(string freelancerId)
    {
        using IDbConnection db = new SqlConnection(connectionString);

        var sql = @"SELECT ID, TransactionID, JsonContent, IngestedAt, NationalId, PlatformName, 'Project' AS Source
                    FROM [FLIPMiddleware].[dbo].[StagingTableProject]
                    WHERE NationalId = @NationalId

                    UNION ALL

                    SELECT ID, TransactionID, JsonContent, IngestedAt, NationalId, PlatformName, 'Ride' AS Source
                    FROM [FLIPMiddleware].[dbo].[StagingTableRide]
                    WHERE NationalId = @NationalId
                    ORDER BY IngestedAt DESC;";

        var result = (await db.QueryAsync<GetFreelancerDto>(sql, new { NationalId = freelancerId })).ToList();

        return result;
    }

    #endregion

}
