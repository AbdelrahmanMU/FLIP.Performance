using Dapper;
using FLIP.Application.Interfaces;
using FLIP.Application.Models;
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

    public async Task<int> UpdateFreelancers(FreelancerData? freelancersData)
    {
        ArgumentNullException.ThrowIfNull(freelancersData);

        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
        UPDATE StagingTableProject
        SET 
            PlatformName = @PlatformName,
            IngestedAt = @IngestedAt,
            JsonContent = @JsonContent
        WHERE NationalId = @NationalId;";

        var affectedRows = await db.ExecuteAsync(sql, freelancersData);

        return affectedRows;
    }

    public async Task<int> UpdateFreelancersRide(FreelancerData? freelancersData)
    {
        ArgumentNullException.ThrowIfNull(freelancersData);

        using IDbConnection db = new SqlConnection(connectionString);

        string sql = @"
        UPDATE StagingTableRide
        SET 
            PlatformName = @PlatformName,
            IngestedAt = @IngestedAt,
            JsonContent = @JsonContent
        WHERE NationalId = @NationalId;";

        var affectedRows = await db.ExecuteAsync(sql, freelancersData);

        return affectedRows;
    }

    #endregion

    #region Get

    public async Task<List<string>> GetFreelancersIds()
    {
        using IDbConnection db = new SqlConnection(connectionString);

        string selectSql = @"
        SELECT NationalId 
        FROM StagingTableProject";

        var result = await db.QueryAsync<string>(selectSql);

        return result.ToList();
    }


    #endregion

}
