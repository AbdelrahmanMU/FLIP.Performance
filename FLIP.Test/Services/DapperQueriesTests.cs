using Moq;
using Xunit;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using FLIP.Infrastructure.Services;
using FLIP.Application.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace FLIP.Tests.Services;

public class DapperQueriesTests
{
    private readonly Mock<IDbConnection> _mockDbConnection;
    private readonly DapperQueries _dapperQueries;

    public DapperQueriesTests()
    {
        // Mock IDbConnection
        _mockDbConnection = new Mock<IDbConnection>();

        // Mock IConfiguration using InMemoryCollection
        var configurationData = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=T-SQL2019-DB; Database=FLIPMiddleware; User ID=moh_absai; Password=kl721@@km&; TrustServerCertificate=True; Connection Timeout=30;" }
            };

        var mockConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        // Instantiate DapperQueries with mocked IConfiguration
        _dapperQueries = new DapperQueries(mockConfiguration);
    }

    #region Insert Tests

    [Fact]
    public async Task InsertFreelancers_ShouldReturnAffectedRows()
    {
        // Arrange
        var freelancersData = new List<FreelancerData>
            {
                new() {
                    TransactionID = Guid.NewGuid(),
                    PlatformName = "Vimeo",
                    IngestedAt = DateTime.Now,
                    NationalId = "12345",
                    JsonContent = "{}"
                }
            };

        // Act
        var result = await _dapperQueries.InsertFreelancers(freelancersData);

        // Assert
        Assert.Equal(1, result); // Verify that 1 row was affected
    }

    [Fact]
    public async Task InsertFreelancersRide_ShouldReturnAffectedRows()
    {
        // Arrange
        var freelancersData = new List<FreelancerData>
            {
                new() {
                    TransactionID = Guid.NewGuid(),
                    PlatformName = "Vimeo",
                    IngestedAt = DateTime.Now,
                    NationalId = "12345",
                    JsonContent = "{}"
                }
            };

        // Act
        var result = await _dapperQueries.InsertFreelancersRide(freelancersData);

        // Assert
        Assert.Equal(1, result); 
    }

    [Fact]
    public async Task InsertLogs_ShouldReturnAffectedRows()
    {
        // Arrange
        var apiLogs = new List<ApiLog>
            {
                new() {
                    RequestUri = "/api/test",
                    StatusCode = 200,
                    Message = "Success",
                    LoggedAt = DateTime.Now,
                    ResponseTimeMs = 100
                }
            };

        // Act
        var result = await _dapperQueries.InsertLogs(apiLogs);

        // Assert
        Assert.Equal(1, result); 
    }

    [Fact]
    public async Task InsertErrorLogs_ShouldReturnAffectedRows()
    {
        // Arrange
        var errorLogs = new List<ErrorLogs>
            {
                new() {
                    RequestUrl = "/api/error",
                    RequestPayload = "{}",
                    ErrorMessage = "Error message",
                    LoggedAt = DateTime.Now
                }
            };

        // Act
        var result = await _dapperQueries.InsertErrorLogs(errorLogs);

        // Assert
        Assert.Equal(1, result); 
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateFreelancers_ShouldReturnAffectedRows()
    {
        // Arrange
        var freelancersData = new List<FreelancerData>
            {
                new() {
                    TransactionID = Guid.NewGuid(),
                    PlatformName = "Vimeo",
                    IngestedAt = DateTime.Now,
                    NationalId = "12345",
                    JsonContent = "{}"
                }
            };

        // Act
        var result = await _dapperQueries.UpdateFreelancers(freelancersData);

        // Assert
        Assert.Equal(1, result); // Verify that 1 row was affected
    }

    [Fact]
    public async Task UpdateFreelancersRide_ShouldReturnAffectedRows()
    {
        // Arrange
        var freelancersData = new List<FreelancerData>
            {
                new() {
                    TransactionID = Guid.NewGuid(),
                    PlatformName = "Vimeo",
                    IngestedAt = DateTime.Now,
                    NationalId = "12345",
                    JsonContent = "{}"
                }
            };

        // Act
        var result = await _dapperQueries.UpdateFreelancersRide(freelancersData);

        // Assert
        Assert.True(result >= 0); 
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetFreelancersIds_ShouldReturnListOfIds()
    {
        // Arrange
        var expectedIds = new List<string> { "1115604133", "12345" };

        // Act
        var result = await _dapperQueries.GetFreelancersIds();

        // Assert
        Assert.True(expectedIds.OrderBy(id => id).SequenceEqual(result.OrderBy(id => id)),
        "The returned IDs do not match the expected list, ignoring order.");
    }

    #endregion
}
